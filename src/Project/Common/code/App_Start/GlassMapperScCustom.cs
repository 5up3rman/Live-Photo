#region GlassMapperScCustom generated code

using Glass.Mapper.Configuration;
using Glass.Mapper.IoC;
using Glass.Mapper.Maps;
using Glass.Mapper.Sc.Configuration.Attributes;
using Glass.Mapper.Sc.IoC;
using Paragon.Foundation.LivePhoto.DataMappers;
using IDependencyResolver = Glass.Mapper.Sc.IoC.IDependencyResolver;

namespace Paragon.Common.LivePhoto.App_Start
{
    public static  class GlassMapperScCustom
    {
		public static IDependencyResolver CreateResolver(){
			var config = new Glass.Mapper.Sc.Config();

			var dependencyResolver = new DependencyResolver(config);
            dependencyResolver.DataMapperFactory.Insert(0, () => new LivePhotoDataMapper());

            return dependencyResolver;
		}

		public static IConfigurationLoader[] GlassLoaders()
		{
		    return new IConfigurationLoader[]
            {
                new SitecoreAttributeConfigurationLoader("Paragon.Foundation.Models"),
                new SitecoreAttributeConfigurationLoader("Paragon.Foundation.LivePhoto"),
                new SitecoreAttributeConfigurationLoader("Paragon.Demo.LivePhoto")
		    };
		}
		public static void PostLoad(){
			//Remove the comments to activate CodeFist
			/* CODE FIRST START
            var dbs = Sitecore.Configuration.Factory.GetDatabases();
            foreach (var db in dbs)
            {
                var provider = db.GetDataProviders().FirstOrDefault(x => x is GlassDataProvider) as GlassDataProvider;
                if (provider != null)
                {
                    using (new SecurityDisabler())
                    {
                        provider.Initialise(db);
                    }
                }
            }
             * CODE FIRST END
             */
		}
		public static void AddMaps(IConfigFactory<IGlassMap> mapsConfigFactory)
        {
			// Add maps here
            // mapsConfigFactory.Add(() => new SeoMap());
        }
    }
}
#endregion