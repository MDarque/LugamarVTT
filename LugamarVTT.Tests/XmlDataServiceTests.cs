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
            Assert.Equal(2, character.CombatManeuverDefense.DexBonus);
            Assert.Equal(1, character.CombatManeuverDefense.SizeBonus);
            Assert.Equal(3, character.CombatManeuverDefense.Misc);
            Assert.Equal(20, character.CombatManeuverDefense.Total);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
