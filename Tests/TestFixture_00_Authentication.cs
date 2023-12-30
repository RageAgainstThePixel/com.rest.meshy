// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_00_Authentication
    {
        [SetUp]
        public void Setup()
        {
            var authJson = new MeshyAuthInfo("msy_test12");
            var authText = JsonUtility.ToJson(authJson, true);
            File.WriteAllText(MeshyAuthentication.CONFIG_FILE, authText);
        }

        [Test]
        public void Test_01_GetAuthFromEnv()
        {
            var auth = new MeshyAuthentication().LoadFromEnvironment();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
        }

        [Test]
        public void Test_02_GetAuthFromFile()
        {
            var auth = new MeshyAuthentication().LoadFromPath(Path.GetFullPath(MeshyAuthentication.CONFIG_FILE));
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("msy_test12", auth.Info.ApiKey);
        }

        [Test]
        public void Test_03_GetAuthFromNonExistentFile()
        {
            var auth = new MeshyAuthentication().LoadFromDirectory(filename: "bad.config");
            Assert.IsNull(auth);
        }

        [Test]
        public void Test_04_GetAuthFromConfiguration()
        {
            var configPath = $"Assets/Resources/{nameof(MeshyConfiguration)}.asset";
            var cleanup = false;

            if (!File.Exists(Path.GetFullPath(configPath)))
            {
                if (!Directory.Exists($"{Application.dataPath}/Resources"))
                {
                    Directory.CreateDirectory($"{Application.dataPath}/Resources");
                }

                var instance = ScriptableObject.CreateInstance<MeshyConfiguration>();
                instance.ApiKey = "msy_test12";
                AssetDatabase.CreateAsset(instance, configPath);
                cleanup = true;
            }

            var configuration = AssetDatabase.LoadAssetAtPath<MeshyConfiguration>(configPath);
            Assert.IsNotNull(configuration);
            var auth = new MeshyAuthentication().LoadFromAsset(configuration);

            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
            Assert.AreEqual(auth.Info.ApiKey, configuration.ApiKey);

            if (cleanup)
            {
                AssetDatabase.DeleteAsset(configPath);
                AssetDatabase.DeleteAsset("Assets/Resources");
            }
        }

        [Test]
        public void Test_05_Authentication()
        {
            var defaultAuth = MeshyAuthentication.Default;

            Assert.IsNotNull(defaultAuth);
            Assert.IsNotNull(defaultAuth.Info.ApiKey);
            Assert.AreEqual(defaultAuth.Info.ApiKey, MeshyAuthentication.Default.Info.ApiKey);

            var manualAuth = new MeshyAuthentication("msy_testAA");
            Assert.IsNotNull(manualAuth);
            Assert.IsNotNull(manualAuth.Info.ApiKey);
            Assert.AreEqual(manualAuth.Info.ApiKey, MeshyAuthentication.Default.Info.ApiKey);

            MeshyAuthentication.Default = defaultAuth;
        }

        [Test]
        public void Test_06_GetKey()
        {
            var auth = new MeshyAuthentication("msy_testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("msy_testAA", auth.Info.ApiKey);
        }

        [Test]
        public void Test_07_GetKeyFailed()
        {
            MeshyAuthentication auth = null;

            try
            {
                auth = new MeshyAuthentication("fail-key");
            }
            catch (InvalidCredentialException)
            {
                Assert.IsNull(auth);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, $"Expected exception {nameof(InvalidCredentialException)} but got {e.GetType().Name}");
            }
        }

        [Test]
        public void Test_08_ParseKey()
        {
            var auth = new MeshyAuthentication("msy_testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("msy_testAA", auth.Info.ApiKey);
            auth = "msy_testCC";
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("msy_testCC", auth.Info.ApiKey);

            auth = new MeshyAuthentication("msy_testBB");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("msy_testBB", auth.Info.ApiKey);
        }

        [Test]
        public void Test_12_CustomDomainConfigurationSettings()
        {
            var auth = new MeshyAuthentication("customIssuedToken");
            var settings = new MeshySettings(domain: "api.your-custom-domain.com");
            var api = new MeshyClient(auth, settings);
            Debug.Log(api.Settings.Info.BaseRequestUrlFormat);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(MeshyAuthentication.CONFIG_FILE))
            {
                File.Delete(MeshyAuthentication.CONFIG_FILE);
            }


            MeshySettings.Default = null;
            MeshyAuthentication.Default = null;
        }
    }
}
