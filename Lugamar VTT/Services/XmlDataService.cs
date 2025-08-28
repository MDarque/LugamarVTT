using LugamarVTT.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LugamarVTT.Services
{
    /// <summary>
    /// Provides access to data stored within an XML database.  The file
    /// referenced must exist on disk; by default it looks for a file named
    /// <c>db.xml</c> in the application base directory.  Results are parsed
    /// into <see cref="Character"/> objects representing Pathfinder 1e player
    /// characters.
    /// </summary>
    public class XmlDataService
    {
        private readonly ILogger<XmlDataService> _logger;
        private readonly IHostEnvironment _environment;
        private XDocument? _xmlDocument;
        private DateTime _lastReadTime;

        public XmlDataService(ILogger<XmlDataService> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Load and cache the XML document from disk.  If the file has changed
        /// since the last read, the cache is refreshed.  Throws if the file
        /// cannot be found.
        /// </summary>
        private void EnsureLoaded()
        {
            // Determine the path to the XML database (db.xml).  If the file is
            // not found in the content root, try the current working directory.
            var basePath = _environment.ContentRootPath;
            var filePath = Path.Combine(basePath, "db.xml");
            if (!File.Exists(filePath))
            {
                // Fall back to the executing directory
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.xml");
            }

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"XML database not found at '{fileInfo.FullName}'.");
            }

            // Reload if the file changed since last read
            if (_xmlDocument == null || fileInfo.LastWriteTimeUtc > _lastReadTime)
            {
                _logger.LogInformation("Loading XML database from {Path}", fileInfo.FullName);
                using var stream = File.OpenRead(fileInfo.FullName);
                _xmlDocument = XDocument.Load(stream);
                _lastReadTime = fileInfo.LastWriteTimeUtc;
            }
        }

        /// <summary>
        /// Get all characters defined within the XML database.  Each &lt;charsheet&gt;
        /// element is converted into a <see cref="Character"/> instance.  If
        /// expected elements are missing from the XML, default values are used.
        /// </summary>
        public IEnumerable<Character> GetCharacters()
        {
            EnsureLoaded();
            if (_xmlDocument == null)
            {
                yield break;
            }

            var root = _xmlDocument.Root;
            if (root == null)
            {
                yield break;
            }

            // Fantasy Grounds stores all player characters inside a single
            // <charsheet> element where each child element whose name starts
            // with "id-" represents an individual character. The previous
            // implementation treated each <charsheet> as an individual
            // character which meant only the first entry was surfaced.
            // Enumerate those dynamic id nodes directly so every character in
            // the database is returned.
            var charsheetRoot = root.Element("charsheet");
            if (charsheetRoot == null)
            {
                yield break;
            }

            var sheets = charsheetRoot.Elements()
                                       .Where(e => e.Name.LocalName.StartsWith("id-"))
                                       .ToList();

            // Some test fixtures or simplified XML may omit the dynamic id
            // wrapper and place character fields directly beneath
            // <charsheet>.  If no id-prefixed elements are found, treat each
            // direct child as a character definition.
            if (sheets.Count == 0)
            {
                sheets = new List<XElement> { charsheetRoot };
            }

            for (var i = 0; i < sheets.Count; i++)
            {
                var character = ParseCharacter(sheets[i], i);
                yield return character;
            }
        }

        /// <summary>
        /// Convert the raw <c>&lt;charsheet&gt;</c> node into a <see cref="Character"/>
        /// instance.  The XML produced by Fantasy Grounds nests the real data
        /// inside a child element with a dynamic name (e.g. <c>&lt;id-00001&gt;</c>).
        /// This helper normalises that structure and safely extracts commonly
        /// used fields such as ability scores and combat stats.  Any missing
        /// information is defaulted to sensible values so that the web site can
        /// still render a sheet for partially populated characters.
        /// </summary>
        private static Character ParseCharacter(XElement sheet, int id)
        {
            // Fantasy Grounds stores the actual character information inside a
            // child element whose name starts with "id-".  Grab that node if it
            // exists; otherwise fall back to the sheet itself.
            var charNode = sheet.Elements()
                                 .FirstOrDefault(e => e.Name.LocalName.StartsWith("id-"))
                           ?? sheet;

            // Helper local functions to read integers and strings safely.
            static int GetInt(XElement? el) => int.TryParse(el?.Value, out var v) ? v : 0;
            static string? GetString(XElement? el) => el?.Value;
            static string GetFormatted(XElement? el) => el == null
                ? string.Empty
                : string.Concat(el.Nodes().Select(n => n.ToString()));
            static int AbilityMod(int score) => (int)Math.Floor((score - 10) / 2.0);

            // Ability scores are nested within <abilities>/<ability>/<score>.
            var abilities = charNode.Element("abilities");

            // Character classes
            var classesNode = charNode.Element("classes");
            var classList = classesNode?.Elements()
                .Where(e => e.Name.LocalName.StartsWith("id-"))
                .Select(e => new CharacterClass
                {
                    Name = GetString(e.Element("name")),
                    Level = GetInt(e.Element("level")),
                    Favored = GetInt(e.Element("favored")) == 1,
                    SkillRanks = GetInt(e.Element("skillranks")),
                    SkillRanksUsed = GetInt(e.Element("skillranksused"))
                })
                .ToList() ?? new List<CharacterClass>();

            var attackNode = charNode.Element("attackbonus");
            int baseAttack = GetInt(attackNode?.Element("base"));

            var character = new Character
            {
                Id = id,
                Name = GetString(charNode.Element("name")),
                Gender = GetString(charNode.Element("gender")),
                Age = GetString(charNode.Element("age")),
                Height = GetString(charNode.Element("height")),
                Weight = GetString(charNode.Element("weight")),
                Size = GetString(charNode.Element("size")),
                Alignment = GetString(charNode.Element("alignment")),
                Deity = GetString(charNode.Element("deity")),
                Race = GetString(charNode.Element("race")),
                Class = classList.FirstOrDefault()?.Name,
                Level = GetInt(charNode.Element("level")),
                Experience = GetInt(charNode.Element("exp")),
                ExperienceNeeded = GetInt(charNode.Element("expneeded")),
                Classes = classList,
                ArmorClass = GetInt(charNode.Element("ac")?
                                            .Element("totals")?
                                            .Element("general")),
                TouchArmorClass = GetInt(charNode.Element("ac")?
                                                .Element("totals")?
                                                .Element("touch")),
                FlatFootedArmorClass = GetInt(charNode.Element("ac")?
                                                .Element("totals")?
                                                .Element("flatfooted")),
                HitPoints = GetInt(charNode.Element("hp")?.Element("total")),
                CurrentHitPoints = GetInt(charNode.Element("hp")?.Element("current")),
                Fortitude = GetInt(charNode.Element("saves")?.Element("fortitude")?.Element("total")),
                Reflex = GetInt(charNode.Element("saves")?.Element("reflex")?.Element("total")),
                Will = GetInt(charNode.Element("saves")?.Element("will")?.Element("total")),
                Initiative = GetInt(charNode.Element("initiative")?.Element("total")),
                Speed = GetInt(charNode.Element("speed")?.Element("total")),
                BaseAttackBonus = baseAttack,
                SkillPointsSpent = GetInt(charNode.Element("skillpoints")?.Element("spent"))
            };

            // Parse ability scores with modifiers and permanent adjustments
            var abilityNames = new[] { "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma" };
            foreach (var name in abilityNames)
            {
                var node = abilities?.Element(name);
                if (node == null)
                {
                    character.Abilities[name] = new AbilityScore();
                    continue;
                }

                var perms = node.Element("abilperms")?.Elements()
                    .Where(e => e.Name.LocalName.StartsWith("id-"))
                    .Select(e => new AbilityPerm
                    {
                        PermNum = GetInt(e.Element("permnum")),
                        BonusType = GetString(e.Element("bonus_type")),
                        Name = GetString(e.Element("name"))
                    })
                    .ToList() ?? new List<AbilityPerm>();

                character.Abilities[name] = new AbilityScore
                {
                    Score = GetInt(node.Element("score")),
                    Bonus = GetInt(node.Element("bonus")),
                    Base = GetInt(node.Element("base")),
                    Damage = GetInt(node.Element("damage")),
                    Perm = GetInt(node.Element("perm")),
                    Perms = perms
                };
            }

            if (attackNode != null)
            {
                var meleeNode = attackNode.Element("melee");
                if (meleeNode != null)
                {
                    character.MeleeAttackBonus = new AttackBonusDetail
                    {
                        BaseAttackBonus = baseAttack,
                        AbilityMod = GetInt(meleeNode.Element("abilitymod")),
                        SizeBonus = GetInt(meleeNode.Element("size")),
                        Misc = GetInt(meleeNode.Element("misc")),
                        Temp = GetInt(meleeNode.Element("temporary")),
                        Total = GetInt(meleeNode.Element("total"))
                    };
                }

                var rangedNode = attackNode.Element("ranged");
                if (rangedNode != null)
                {
                    character.RangedAttackBonus = new AttackBonusDetail
                    {
                        BaseAttackBonus = baseAttack,
                        AbilityMod = GetInt(rangedNode.Element("abilitymod")),
                        SizeBonus = GetInt(rangedNode.Element("size")),
                        Misc = GetInt(rangedNode.Element("misc")),
                        Temp = GetInt(rangedNode.Element("temporary")),
                        Total = GetInt(rangedNode.Element("total"))
                    };
                }

                var grappleNode = attackNode.Element("grapple");
                if (grappleNode != null)
                {
                    character.CombatManeuverBonus = new AttackBonusDetail
                    {
                        BaseAttackBonus = baseAttack,
                        AbilityMod = GetInt(grappleNode.Element("abilitymod")),
                        SizeBonus = GetInt(grappleNode.Element("size")),
                        Misc = GetInt(grappleNode.Element("misc")),
                        Temp = GetInt(grappleNode.Element("temporary")),
                        Total = GetInt(grappleNode.Element("total"))
                    };
                }
            }

            var acNode = charNode.Element("ac");
            if (acNode != null)
            {
                var sources = acNode.Element("sources");
                int abilityMod = GetInt(sources?.Element("abilitymod"));
                int abilityMod2 = GetInt(sources?.Element("abilitymod2"));
                int sizeMod = GetInt(sources?.Element("size"));
                int armorBonus = GetInt(sources?.Element("armor"));
                int shieldBonus = GetInt(sources?.Element("shield"));
                int naturalArmor = GetInt(sources?.Element("naturalarmor"));
                int dodge = GetInt(sources?.Element("dodge"));
                int misc = GetInt(sources?.Element("misc"));
                int deflection = GetInt(sources?.Element("deflection"));
                int temp = GetInt(sources?.Element("temporary"));
                int touchMisc = GetInt(sources?.Element("touchmisc"));
                int ffMisc = GetInt(sources?.Element("ffmisc"));
                int cmdBase = GetInt(sources?.Element("cmdbasemod"));
                int cmdabilitymod = GetInt(sources?.Element("cmdabilitymod"));
                int cmdStr = cmdabilitymod;
                int cmdDex = cmdBase;
                int cmdMisc = GetInt(sources?.Element("cmdmisc"));

                int baseMisc = misc + abilityMod2;

                var totals = acNode.Element("totals");
                int generalTotal = GetInt(totals?.Element("general"));
                int touchTotal = GetInt(totals?.Element("touch"));
                int flatTotal = GetInt(totals?.Element("flatfooted"));
                int cmdTotal = GetInt(totals?.Element("cmd"));

                character.ArmorClassBreakdown = new ArmorClassDetail
                {
                    DexModifier = abilityMod,
                    SizeModifier = sizeMod,
                    ArmorBonus = armorBonus,
                    ShieldBonus = shieldBonus,
                    NaturalArmor = naturalArmor,
                    Dodge = dodge,
                    Misc = baseMisc,
                    Deflection = deflection,
                    Temp = temp,
                    Total = generalTotal
                };

                character.TouchArmorClassBreakdown = new ArmorClassDetail
                {
                    DexModifier = abilityMod,
                    SizeModifier = sizeMod,
                    ArmorBonus = 0,
                    ShieldBonus = 0,
                    NaturalArmor = 0,
                    Dodge = dodge,
                    Misc = baseMisc + touchMisc,
                    Deflection = deflection,
                    Temp = temp,
                    Total = touchTotal
                };

                character.FlatFootedArmorClassBreakdown = new ArmorClassDetail
                {
                    DexModifier = 0,
                    SizeModifier = sizeMod,
                    ArmorBonus = armorBonus,
                    ShieldBonus = shieldBonus,
                    NaturalArmor = naturalArmor,
                    Dodge = 0,
                    Misc = baseMisc + ffMisc,
                    Deflection = deflection,
                    Temp = temp,
                    Total = flatTotal
                };

                character.CombatManeuverDefense = new CmdDetail
                {
                    BaseAttackBonus = cmdBase,
                    StrBonus = cmdStr,
                    DexBonus = cmdDex,
                    SizeBonus = sizeMod,
                    Misc = cmdMisc,
                    Total = cmdTotal
                };
            }

            // Optional collections: skills, feats, equipment and spells can
            // appear at various depths, so search the entire character node.
            character.Skills.AddRange(
                charNode.Descendants("skill").Select(e => (string?)e.Attribute("name") ?? e.Value));

            var skillList = charNode.Element("skilllist");
            if (skillList != null)
            {
                foreach (var skill in skillList.Elements())
                {
                    var label = GetString(skill.Element("label")) ?? string.Empty;
                    var sub = GetString(skill.Element("sublabel"));
                    var ranks = GetInt(skill.Element("ranks"));
                    var misc = GetInt(skill.Element("misc"));
                    var statName = (GetString(skill.Element("statname")) ?? string.Empty).ToLowerInvariant();

                    character.Abilities.TryGetValue(statName, out var abilityDetail);
                    int abilityScore = abilityDetail?.Score ?? 0;
                    int abilityMod = abilityDetail?.Bonus ?? AbilityMod(abilityScore);
                    var abilityAbbrev = statName switch
                    {
                        "strength" => "STR",
                        "dexterity" => "DEX",
                        "constitution" => "CON",
                        "intelligence" => "INT",
                        "wisdom" => "WIS",
                        "charisma" => "CHA",
                        _ => string.Empty
                    };

                    var total = ranks + abilityMod + misc;
                    var name = string.IsNullOrWhiteSpace(sub) ? label : $"{label} ({sub})";
                    character.Skills.Add(name);
                    character.SkillDetails.Add(new SkillDetail
                    {
                        Name = name,
                        Ability = abilityAbbrev,
                        AbilityBonus = abilityMod,
                        Ranks = ranks,
                        Misc = misc,
                        Total = total
                    });
                }
            }

            // Special abilities and traits
            var specialList = charNode.Element("specialabilitylist");
            if (specialList != null)
            {
                foreach (var ability in specialList.Elements())
                {
                    var name = GetString(ability.Element("name")) ?? string.Empty;
                    var source = GetString(ability.Element("source")) ?? string.Empty;
                    var type = GetString(ability.Element("type")) ?? string.Empty;
                    var text = GetFormatted(ability.Element("text"));

                    if (!string.IsNullOrEmpty(type) && type.Contains("Trait", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var traitSource = type.Replace("Trait - ", string.Empty, System.StringComparison.OrdinalIgnoreCase);
                        character.Traits.Add(new Trait
                        {
                            Name = name,
                            Source = traitSource,
                            Text = text
                        });
                    }
                    else
                    {
                        character.SpecialAbilities.Add(new SpecialAbility
                        {
                            Name = name,
                            Source = source,
                            Text = text
                        });
                    }
                }
            }

            // Feats
            var featList = charNode.Element("featlist");
            if (featList != null)
            {
                foreach (var feat in featList.Elements())
                {
                    var name = GetString(feat.Element("name")) ?? string.Empty;
                    var summary = GetString(feat.Element("summary")) ?? string.Empty;
                    var type = GetString(feat.Element("type")) ?? string.Empty;
                    var prereq = GetString(feat.Element("prerequisites")) ?? string.Empty;
                    var benefit = GetFormatted(feat.Element("benefit"));
                    var normal = GetFormatted(feat.Element("normal"));
                    var special = GetFormatted(feat.Element("special"));

                    character.Feats.Add(name);
                    character.FeatDetails.Add(new FeatDetail
                    {
                        Name = name,
                        Summary = summary,
                        Type = type,
                        Prerequisites = prereq,
                        Benefit = benefit,
                        Normal = normal,
                        Special = special
                    });
                }
            }

            // Proficiencies
            var profList = charNode.Element("proficiencylist");
            if (profList != null)
            {
                foreach (var prof in profList.Elements())
                {
                    var name = GetString(prof.Element("name"));
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        character.Proficiencies.Add(name);
                    }
                }
            }

            // Equipment
            var inventory = charNode.Element("inventorylist");
            if (inventory != null)
            {
                foreach (var item in inventory.Elements())
                {
                    var name = GetString(item.Element("name")) ?? string.Empty;
                    var type = GetString(item.Element("type")) ?? string.Empty;
                    var subtype = GetString(item.Element("subtype")) ?? string.Empty;
                    var cost = GetString(item.Element("cost")) ?? string.Empty;
                    var weight = GetString(item.Element("weight")) ?? string.Empty;
                    var desc = GetFormatted(item.Element("description"));

                    character.Equipment.Add(name);
                    character.EquipmentDetails.Add(new EquipmentItem
                    {
                        Name = name,
                        Type = type,
                        Subtype = subtype,
                        Cost = cost,
                        Weight = weight,
                        Description = desc
                    });
                }
            }

            character.Spells.AddRange(
                charNode.Descendants("spell").Select(e => (string?)e.Attribute("name") ?? e.Value));

            return character;
        }

        /// <summary>
        /// Retrieve a single character by its identifier.  If the id is out
        /// of bounds or no characters exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="id">Zero‑based identifier assigned by <see cref="GetCharacters"/>.</param>
        public Character? GetCharacterById(int id)
        {
            // Force materialisation of the collection to ensure consistent Ids
            var characters = GetCharacters().ToList();
            return id >= 0 && id < characters.Count ? characters[id] : null;
        }
    }
}
