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

            var charsheets = root.Descendants("charsheet");
            foreach (var sheet in charsheets)
            {
                var character = new Character
                {
                    Name = (string?)sheet.Element("name"),
                    Race = (string?)sheet.Element("race"),
                    Class = (string?)sheet.Element("class"),
                    Alignment = (string?)sheet.Element("alignment"),
                    Level = int.TryParse((string?)sheet.Element("level"), out var lvl) ? lvl : 0,
                    Strength = int.TryParse((string?)sheet.Element("strength"), out var str) ? str : 0,
                    Dexterity = int.TryParse((string?)sheet.Element("dexterity"), out var dex) ? dex : 0,
                    Constitution = int.TryParse((string?)sheet.Element("constitution"), out var con) ? con : 0,
                    Intelligence = int.TryParse((string?)sheet.Element("intelligence"), out var intel) ? intel : 0,
                    Wisdom = int.TryParse((string?)sheet.Element("wisdom"), out var wis) ? wis : 0,
                    Charisma = int.TryParse((string?)sheet.Element("charisma"), out var cha) ? cha : 0,
                    ArmorClass = int.TryParse((string?)sheet.Element("ac"), out var ac) ? ac : 0,
                    HitPoints = int.TryParse((string?)sheet.Element("hp"), out var hp) ? hp : 0,
                    BaseAttackBonus = (string?)sheet.Element("bab"),
                };

                // Optional lists may be nested within grouping elements (e.g.,
                // <skills><skill /></skills>).  Using Descendants instead of
                // Elements ensures we capture items no matter their depth
                // beneath the <charsheet> node.
                character.Skills.AddRange(
                    sheet.Descendants("skill").Select(e => (string?)e.Attribute("name") ?? e.Value));
                character.Feats.AddRange(
                    sheet.Descendants("feat").Select(e => (string?)e.Attribute("name") ?? e.Value));
                character.Equipment.AddRange(
                    sheet.Descendants("item").Select(e => (string?)e.Attribute("name") ?? e.Value));
                character.Spells.AddRange(
                    sheet.Descendants("spell").Select(e => (string?)e.Attribute("name") ?? e.Value));

                yield return character;
            }
        }
    }
}