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
    <feats>
      <feat name="Dodge" />
    </feats>
    <equipment>
      <items>
        <item name="Sword" />
      </items>
    </equipment>
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
}
