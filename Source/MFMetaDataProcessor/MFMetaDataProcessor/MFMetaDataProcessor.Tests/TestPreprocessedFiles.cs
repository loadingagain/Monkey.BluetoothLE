﻿using System;
using System.IO;
using Mono.Cecil;
using NUnit.Framework;

namespace MFMetaDataProcessor.Tests
{
    [TestFixture]
    public sealed class TestPreprocessedFiles
    {
        [Test]
        [Ignore("Temporary ignore - stack size calculation issue not solved.")]
        public void ClockSampleTest()
        {
            TestSingleAssembly("Clock");
        }

        [Test]
        public void ExtendedWeakReferencesTest()
        {
            TestSingleAssembly("ExtendedWeakReferences");
        }

        [Test]
        public void FileSystemSampleTest()
        {
            TestSingleAssembly("FileSystemSample");
        }

        private static void TestSingleAssembly(
            String name)
        {
            TestSingleAssembly(name, "le", TinyBinaryWriter.CreateLittleEndianBinaryWriter);
            TestSingleAssembly(name, "be", TinyBinaryWriter.CreateBigEndianBinaryWriter);
        }

        private static void TestSingleAssembly (
            String name, String endianness,
            Func<BinaryWriter, TinyBinaryWriter> getBinaryWriter)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(
                String.Format(@"Data\{0}\{0}.exe", Path.GetFileName(name)));

            var fileName = ProcessSingleFile(name, endianness,
                assemblyDefinition, getBinaryWriter);

            CompareFiles(fileName);
        }

         private static String ProcessSingleFile (
            String name, String subDirectoryName,
            AssemblyDefinition assemblyDefinition,
            Func<BinaryWriter, TinyBinaryWriter> getBinaryWriter)
        {
            var fileName = String.Format(@"Data\{0}\{1}\{0}.pex", name, subDirectoryName);

            using (var stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new BinaryWriter(stream))
            {
                new TinyAssemblyBuilder(assemblyDefinition)
                    .Write(getBinaryWriter(writer));
            }

            return fileName;
        }

        private static void CompareFiles (String name)
        {
            var expetedFileName = Path.ChangeExtension(name, ".pe");

            var expectedBytes = File.ReadAllBytes(expetedFileName);
            var realBytes = File.ReadAllBytes(name);

            Assert.AreEqual(expectedBytes.Length, realBytes.Length,
                "Size is not equal for file " + name);
            Assert.AreEqual(expectedBytes, realBytes,
                "Data is not equal for file " + name);
        }
    }
}
