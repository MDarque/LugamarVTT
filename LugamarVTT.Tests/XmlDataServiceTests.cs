using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using LugamarVTT.Services;
using Xunit;

namespace LugamarVTT.Tests;

public class XmlDataServiceTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; }
            = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public TestHostEnvironment(string contentRoot)
        {
            ContentRootPath = contentRoot;
        }
    }

    [Fact]
    public void GetCharacters_ParsesNestedLists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <name>Hero</name>
    <skills>
      <skill name="Acrobatics" />
    </skills>
    <featlist>
      <id-00001>
        <name>Dodge</name>
      </id-00001>
    </featlist>
    <inventorylist>
      <id-00001>
        <name>Sword</name>
      </id-00001>
    </inventorylist>
    <spells>
      <known>
        <spell name="Magic Missile" />
      </known>
    </spells>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            Assert.Contains("Acrobatics", character.Skills);
            Assert.Contains("Dodge", character.Feats);
            Assert.Contains("Sword", character.Equipment);
            Assert.Contains("Magic Missile", character.Spells);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetCharacters_ParsesArmorClassDeflectionAndTemp()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <ac>
      <sources>
        <abilitymod>1</abilitymod>
        <size>0</size>
        <armor>4</armor>
        <shield>2</shield>
        <naturalarmor>1</naturalarmor>
        <dodge>1</dodge>
        <misc>2</misc>
        <deflection>3</deflection>
        <temporary>4</temporary>
        <abilitymod2>0</abilitymod2>
        <touchmisc>0</touchmisc>
        <ffmisc>0</ffmisc>
      </sources>
      <totals>
        <general>16</general>
        <touch>12</touch>
        <flatfooted>15</flatfooted>
      </totals>
    </ac>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            Assert.Equal(3, character.ArmorClassBreakdown.Deflection);
            Assert.Equal(4, character.ArmorClassBreakdown.Temp);
            Assert.Equal(2, character.ArmorClassBreakdown.Misc);
            Assert.Equal(3, character.TouchArmorClassBreakdown.Deflection);
            Assert.Equal(4, character.TouchArmorClassBreakdown.Temp);
            Assert.Equal(3, character.FlatFootedArmorClassBreakdown.Deflection);
            Assert.Equal(4, character.FlatFootedArmorClassBreakdown.Temp);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetCharacters_ParsesAttackBonusesAndCmd()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <ac>
      <sources>
        <abilitymod>1</abilitymod>
        <abilitymod2>2</abilitymod2>
        <size>1</size>
        <cmdbasemod>3</cmdbasemod>
        <cmdabilitymod>1</cmdabilitymod>
        <cmdmisc>3</cmdmisc>
      </sources>
      <totals>
        <cmd>20</cmd>
        <general>0</general>
        <touch>0</touch>
        <flatfooted>0</flatfooted>
      </totals>
    </ac>
    <attackbonus>
      <base>5</base>
      <melee>
        <abilitymod>1</abilitymod>
        <size>0</size>
        <misc>2</misc>
        <temporary>0</temporary>
        <total>8</total>
      </melee>
      <ranged>
        <abilitymod>3</abilitymod>
        <size>0</size>
        <misc>1</misc>
        <temporary>0</temporary>
        <total>9</total>
      </ranged>
      <grapple>
        <abilitymod>2</abilitymod>
        <size>1</size>
        <misc>3</misc>
        <temporary>0</temporary>
        <total>11</total>
      </grapple>
    </attackbonus>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            Assert.Equal(5, character.BaseAttackBonus);
            Assert.Equal(1, character.MeleeAttackBonus.AbilityMod);
            Assert.Equal(2, character.MeleeAttackBonus.Misc);
            Assert.Equal(3, character.RangedAttackBonus.AbilityMod);
            Assert.Equal(11, character.CombatManeuverBonus.Total);
            Assert.Equal(3, character.CombatManeuverDefense.BaseAttackBonus);
            Assert.Equal(1, character.CombatManeuverDefense.StrBonus);
            Assert.Equal(3, character.CombatManeuverDefense.DexBonus);
            Assert.Equal(1, character.CombatManeuverDefense.SizeBonus);
            Assert.Equal(3, character.CombatManeuverDefense.Misc);
            Assert.Equal(20, character.CombatManeuverDefense.Total);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetCharacters_ParsesAdditionalCharacterInfoAndClasses()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <name>Hero</name>
    <gender>M</gender>
    <age>25</age>
    <height>6'</height>
    <weight>180 lb</weight>
    <size>Medium</size>
    <alignment>NG</alignment>
    <deity>None</deity>
    <race>Human</race>
    <exp>1000</exp>
    <expneeded>2000</expneeded>
    <classes>
      <id-00001>
        <name>Fighter</name>
        <level>2</level>
        <favored>1</favored>
        <skillranks>4</skillranks>
        <skillranksused>2</skillranksused>
      </id-00001>
      <id-00002>
        <name>Wizard</name>
        <level>1</level>
        <favored>0</favored>
        <skillranks>2</skillranks>
        <skillranksused>2</skillranksused>
      </id-00002>
    </classes>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            Assert.Equal("M", character.Gender);
            Assert.Equal("25", character.Age);
            Assert.Equal("6'", character.Height);
            Assert.Equal("180 lb", character.Weight);
            Assert.Equal("Medium", character.Size);
            Assert.Equal("None", character.Deity);
            Assert.Equal(1000, character.Experience);
            Assert.Equal(2000, character.ExperienceNeeded);
            Assert.Equal(2, character.Classes.Count);
            var cls = character.Classes[0];
            Assert.Equal("Fighter", cls.Name);
            Assert.Equal(2, cls.Level);
            Assert.True(cls.Favored);
            Assert.Equal(4, cls.SkillRanks);
            Assert.Equal(2, cls.SkillRanksUsed);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetCharacters_ParsesHitPointsAndDefenses()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <hp>
      <current>10</current>
      <total>12</total>
      <temporary>2</temporary>
      <wounds>1</wounds>
      <nonlethal>3</nonlethal>
    </hp>
    <defenses>
      <damagereduction>5/magic</damagereduction>
      <sr>
        <total>15</total>
      </sr>
      <resistances>fire 10</resistances>
      <immunities>poison</immunities>
      <specialqualities>darkvision</specialqualities>
    </defenses>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            Assert.Equal(12, character.HitPoints);
            Assert.Equal(10, character.CurrentHitPoints);
            Assert.Equal(2, character.TempHitPoints);
            Assert.Equal(1, character.Wounds);
            Assert.Equal(3, character.NonLethalDamage);
            Assert.Equal("5/magic", character.DamageReduction);
            Assert.Equal(15, character.SpellResistance);
            Assert.Equal("fire 10", character.Resistances);
            Assert.Equal("poison", character.Immunities);
            Assert.Equal("darkvision", character.SpecialQualities);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetCharacters_PreservesHtmlInSpecialAbilityText()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "db.xml"), """
<root>
  <charsheet>
    <specialabilitylist>
      <id-00001>
        <name>Shiny Power</name>
        <source>Book</source>
        <type>Ability</type>
        <text>Gives <b>bold</b> strength.</text>
      </id-00001>
    </specialabilitylist>
  </charsheet>
</root>
""");

            var env = new TestHostEnvironment(tempDir);
            var service = new XmlDataService(NullLogger<XmlDataService>.Instance, env);

            var character = service.GetCharacters().Single();

            var ability = Assert.Single(character.SpecialAbilities);
            Assert.Contains("<b>bold</b>", ability.Text.Value);
            Assert.Equal("Gives bold strength.", ability.Summary);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
