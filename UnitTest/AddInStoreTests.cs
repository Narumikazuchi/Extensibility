using Microsoft.VisualStudio.TestTools.UnitTesting;
using Narumikazuchi;
using Narumikazuchi.Extensibility;
using System;
using System.Threading.Tasks;
using TestAddInLib;

namespace UnitTest
{
    [TestClass]
    public class AddInStoreTests
    {
        [TestMethod]
        public void InstantiateStoreTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache();
            instance.RebuildCache();
            instance.UnloadCache();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task TrustedAddInTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            foreach (MethodSignature signature in controller.Methods)
            {
                _context.WriteLine(signature.ToString());
            }
            foreach (PropertySignature signature in controller.Properties)
            {
                _context.WriteLine(signature.ToString());
            }
            foreach (IndexerSignature signature in controller.Indexers)
            {
                _context.WriteLine(signature.ToString());
            }
            Assert.AreEqual(2, controller.Methods.Count);
            Assert.AreEqual(3, controller.Properties.Count);
            Assert.AreEqual(3, controller.Indexers.Count);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            foreach (MethodSignature signature in controller.Methods)
            {
                _context.WriteLine(signature.ToString());
            }
            foreach (PropertySignature signature in controller.Properties)
            {
                _context.WriteLine(signature.ToString());
            }
            foreach (IndexerSignature signature in controller.Indexers)
            {
                _context.WriteLine(signature.ToString());
            }
            Assert.AreEqual(2, controller.Methods.Count);
            Assert.AreEqual(3, controller.Properties.Count);
            Assert.AreEqual(3, controller.Indexers.Count);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInProcedureTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            MethodSignature method = controller.GetMethod("SomeProcedure");
            await controller.InvokeMethodAsync(method);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInProcedureTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            MethodSignature method = controller.GetMethod("SomeProcedure");
            await controller.InvokeMethodAsync(method);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInMethodTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            MethodSignature method = controller.GetMethod("SomeValidation", typeof(Boolean));
            Boolean result = await controller.InvokeMethodAsync<Boolean>(method, true);
            Assert.IsTrue(result);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInMethodTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            MethodSignature method = controller.GetMethod("SomeValidation", typeof(Boolean));
            Boolean result = await controller.InvokeMethodAsync<Boolean>(method, true);
            Assert.IsTrue(result);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInReadOnlyPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeGetOnlyProperty");
            Int32 value = await controller.InvokePropertyGetterAsync<Int32>(property);
            _context.WriteLine(value.ToString());
            Assert.AreEqual(0, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInReadOnlyPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeGetOnlyProperty");
            Int32 value = await controller.InvokePropertyGetterAsync<Int32>(property);
            _context.WriteLine(value.ToString());
            Assert.AreEqual(0, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInSetOnlyPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeSetOnlyProperty");
            await controller.InvokePropertySetterAsync(property, 35);
            property = controller.GetProperty("SomeGetOnlyProperty");
            Int32 value = await controller.InvokePropertyGetterAsync<Int32>(property);
            Assert.AreEqual(35, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInSetOnlyPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeSetOnlyProperty");
            await controller.InvokePropertySetterAsync(property, 35);
            property = controller.GetProperty("SomeGetOnlyProperty");
            Int32 value = await controller.InvokePropertyGetterAsync<Int32>(property);
            Assert.AreEqual(35, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeBothAccessorProperty");
            String value = await controller.InvokePropertyGetterAsync<String>(property);
            _context.WriteLine(value);
            Assert.AreEqual("Test", value);
            await controller.InvokePropertySetterAsync(property, "FooBar");
            value = await controller.InvokePropertyGetterAsync<String>(property);
            _context.WriteLine(value);
            Assert.AreEqual("FooBar", value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInPropertyTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            PropertySignature property = controller.GetProperty("SomeBothAccessorProperty");
            String value = await controller.InvokePropertyGetterAsync<String>(property);
            _context.WriteLine(value);
            Assert.AreEqual("Test", value);
            await controller.InvokePropertySetterAsync(property, "FooBar");
            value = await controller.InvokePropertyGetterAsync<String>(property);
            _context.WriteLine(value);
            Assert.AreEqual("FooBar", value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInReadOnlyIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32), typeof(Int32), typeof(Int32) });
            Double value = await controller.InvokeIndexerGetterAsync<Double>(indexer, 24, 36, 48);
            _context.WriteLine(value.ToString());
            Assert.AreNotEqual(0d, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInReadOnlyIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32), typeof(Int32), typeof(Int32) });
            Double value = await controller.InvokeIndexerGetterAsync<Double>(indexer, 24, 36, 48);
            _context.WriteLine(value.ToString());
            Assert.AreNotEqual(0d, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInSetOnlyIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32), typeof(Int32) });
            await controller.InvokeIndexerSetterAsync(indexer, 0.3125d, 24, 36);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInSetOnlyIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32), typeof(Int32) });
            await controller.InvokeIndexerSetterAsync(indexer, 0.3125d, 24, 36);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task TrustedAddInIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Trusted);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32) });
            await controller.InvokeIndexerSetterAsync(indexer, 0.3125d, 24);
            Double value = await controller.InvokeIndexerGetterAsync<Double>(indexer, 24);
            _context.WriteLine(value.ToString());
            Assert.AreEqual(0.3125d, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        [TestMethod]
        public async Task IsolatedAddInIndexerTest()
        {
            AddInStore<TestAddIn> instance = Singleton<AddInStore<TestAddIn>>.Instance;
            instance.LoadCache(_cache);
            if (!instance.IsAddInRegistered(_addInGuid))
            {
                Boolean registered = instance.Register(_cache, typeof(TestAddIn), AddInSecurity.Isolated);
                Assert.IsTrue(registered);
            }
            IAddInController controller = instance.GetController(_addInGuid);
            Boolean activated = await controller.ActivateAsync();
            Assert.IsTrue(activated);
            IndexerSignature indexer = controller.GetIndexer(new Type[] { typeof(Int32) });
            await controller.InvokeIndexerSetterAsync(indexer, 0.3125d, 24);
            Double value = await controller.InvokeIndexerGetterAsync<Double>(indexer, 24);
            _context.WriteLine(value.ToString());
            Assert.AreEqual(0.3125d, value);
            controller.Shutdown();
            controller.Dispose();
            instance.RebuildCache(_cache);
            instance.UnloadCache(_cache);
        }

        public TestContext TestContext
        {
            get => _context;
            set => _context = value;
        }

        private static TestContext _context;
        private static readonly Guid _addInGuid = Guid.Parse("8849A7A0-2F62-41BB-B687-F4BAF0340E9B");
        private static readonly String _cache = Environment.CurrentDirectory + "\\mycache.addin.cache";
    }
}
