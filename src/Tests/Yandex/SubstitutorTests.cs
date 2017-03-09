using System;
using System.IO;
using GeoCoder.Yandex;
using NUnit.Framework;

namespace Tests.Yandex
{
    [TestFixture]
    public class SubstitutorTests
    {
        private string _path;

        [SetUp]
        public void Context()
        {
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YandexSubstitution.txt");
        }

        [TearDown]
        public void CleanupContext()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }

        [Test]
        public void Should_read_file_copied_from_excel()
        {
            using (var wrtr = File.OpenWrite(_path))
            using (var rdr = GetType().Assembly.GetManifestResourceStream(GetType(), "Substitution.txt"))
            {
                rdr.CopyTo(wrtr);
                wrtr.Flush(true);
            }

            var substitutor = Substitutor.FromFile(_path);
            Assert.AreEqual("Москва, Щукинская улица, 42, ТЦ \"Щукинская\"", substitutor.Substitute("Москва, Щукинская улица, 42"));
            Assert.AreEqual("Москва, Щукинская улица, 43", substitutor.Substitute("Москва, Щукинская улица, 43"));
        }

        [Test]
        public void Should_handle_empty_file()
        {
            File.Create(_path).Dispose();

            var substitutor = Substitutor.FromFile(_path);
            Assert.AreEqual("Москва, Щукинская улица, 42", substitutor.Substitute("Москва, Щукинская улица, 42"));
            Assert.AreEqual("Москва, Щукинская улица, 43", substitutor.Substitute("Москва, Щукинская улица, 43"));
        }

        [Test]
        public void Should_handle_missing_file()
        {
            Assert.IsFalse(File.Exists(_path));

            var substitutor = Substitutor.FromFile(_path);
            Assert.AreEqual("Москва, Щукинская улица, 42", substitutor.Substitute("Москва, Щукинская улица, 42"));
            Assert.AreEqual("Москва, Щукинская улица, 43", substitutor.Substitute("Москва, Щукинская улица, 43"));
        }
    }
}
