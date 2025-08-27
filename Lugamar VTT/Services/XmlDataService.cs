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

            // Enumerate all <charsheet> elements and assign a sequential Id to
            // each character.  This Id is used by the controller to route
            // requests for individual character details.
            var charsheets = root.Descendants("charsheet").ToList();
            for (var i = 0; i < charsheets.Count; i++)
            {
                var sheet = charsheets[i];
                var character = ParseCharacter(sheet, i);
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

            // Ability scores are nested within <abilities>/<ability>/<score>.
            var abilities = charNode.Element("abilities");

            var character = new Character
            {
                Id = id,
                Name = GetString(charNode.Element("name")),
                Race = GetString(charNode.Element("race")),
                Class = GetString(charNode
                    .Element("classes")?
                    .Elements()
                    .FirstOrDefault(e => e.Name.LocalName.StartsWith("id-"))?
                    .Element("name")),
                Alignment = GetString(charNode.Element("alignment")),
                Level = GetInt(charNode.Element("level")),
                Strength = GetInt(abilities?.Element("strength")?.Element("score")),
                Dexterity = GetInt(abilities?.Element("dexterity")?.Element("score")),
                Constitution = GetInt(abilities?.Element("constitution")?.Element("score")),
                Intelligence = GetInt(abilities?.Element("intelligence")?.Element("score")),
                Wisdom = GetInt(abilities?.Element("wisdom")?.Element("score")),
                Charisma = GetInt(abilities?.Element("charisma")?.Element("score")),
                ArmorClass = GetInt(charNode.Element("ac")?
                                            .Element("totals")?
                                            .Element("general")),
                HitPoints = GetInt(charNode.Element("hp")?.Element("total")),
                BaseAttackBonus = GetString(charNode.Element("attackbonus")?.Element("base"))
            };

            // Optional collections: skills, feats, equipment and spells can
            // appear at various depths, so search the entire character node.
            character.Skills.AddRange(
                charNode.Descendants("skill").Select(e => (string?)e.Attribute("name") ?? e.Value));
            character.Feats.AddRange(
                charNode.Descendants("feat").Select(e => (string?)e.Attribute("name") ?? e.Value));
            character.Equipment.AddRange(
                charNode.Descendants("item").Select(e => (string?)e.Attribute("name") ?? e.Value));
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
