using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Ketchup.Web.Mef.Extensions
{
    public class DirectoryDiscoveryCatalog : ComposablePartCatalog
    {
        private readonly object _locker = new object();
        private readonly string _dir;
        private IQueryable<ComposablePartDefinition> _parts;

        public DirectoryDiscoveryCatalog(string directory)
        {
            this._dir = directory;
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                if (_parts == null)
                {
                    lock(_locker)
                    {
                        if (_parts == null)
                        {
                            var parts = LoadParts();

                            Thread.MemoryBarrier();

                            this._parts = parts;
                        }
                    }
                }

                return this._parts;
            }

        }

        private IQueryable<ComposablePartDefinition> LoadParts()
        {
            if (!Directory.Exists(_dir))
            {
                return (new ComposablePartDefinition[0]).AsQueryable();
            }

            var parts = new List<ComposablePartDefinition>();

            foreach(var file in Directory.GetFiles(this._dir, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);

                    using(var catalog = new AssemblyDiscoveryCatalog(assembly))
                    {
                        parts.AddRange(catalog.Parts);
                    }
                }
                catch (Exception)
                {
                    // TODO: we should log or trace
                }
            }

            return parts.AsQueryable();
        }
    }
}
