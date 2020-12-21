using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace SB.Shared.Extensions
{
    public static class EntityExtension
    {
        /// <summary>
        /// Create new Instance Entity Base
        /// </summary>
        /// <typeparam name="T">Entity Base</typeparam>
        /// <param name="entity">Entity CRM</param>
        /// <param name="service">IOrganizationService</param>
        /// <returns></returns>
        public static T ToEntity<T>(this Entity entity, IOrganizationService service) where T : EntityBase
        {
            return (T)Activator.CreateInstance(typeof(T), entity, service);
        }

        public static List<T> ToEntityList<T>(this EntityCollection collection, IOrganizationService service)
            where T : EntityBase
        {
            var list = collection.Entities.ToList();

            return list.ConvertAll(x => x.ToEntity<T>(service));
        }

        public static Entity Merge(this Entity target, Entity source)
        {
            foreach (var attribute in source.Attributes.ToList())
            {
                target[attribute.Key] = source[attribute.Key];
            }

            return target;
        }
    }
}
