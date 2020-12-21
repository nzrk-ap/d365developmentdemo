using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace SB.Shared
{
    public class EntityBase
    {
        private IDictionary<string, object> _changes = new Dictionary<string, object>();
        private Guid? _entityGuid;
        private readonly string _logicalName;
        protected readonly IOrganizationService _service;
        // Private Attributes
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        // Public Attributes
        private bool _preOperation = false;
        private Entity _targetEntity = null;

        // Constructors

        /// <summary>
        ///     Constructor used to setup an exisitng entity
        /// </summary>
        /// <param name="record"></param>
        /// <param name="service"></param>
        protected EntityBase(Entity record, IOrganizationService service)
        {
            // Process ID
            _entityGuid = record.Id;
            _logicalName = record.LogicalName;
            _service = service;

            // Process attributes
            foreach (var attribute in record.Attributes)
                _values.Add(attribute);
        }

        /// <summary>
        ///     Constructor for a new entity.
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="service"></param>
        protected EntityBase(string logicalName, IOrganizationService service = null)
        {
            _logicalName = logicalName;
            _service = service;
        }

        /// <summary>
        ///     Constructor for existing entity - however, get no attributes!
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="id"></param>
        /// <param name="service"></param>
        protected EntityBase(string logicalName, Guid id, IOrganizationService service)
        {
            _logicalName = logicalName;
            _entityGuid = id;
            _service = service;
        }

        // Public Atttributes
        public Guid? Id
        {
            get { return _entityGuid; }
            set { _entityGuid = value; }
        }

        // Accessor for attributes
        public object this[string attributeName]
        {
            get
            {
                return _values.ContainsKey(attributeName)
                    ? _values[attributeName]
                    : null;
            }
            set
            {
                // Set the current value
                if (_values.ContainsKey(attributeName))
                    _values[attributeName] = value;
                else
                    _values.Add(attributeName, value);

                // Set the changeset
                if (_changes.ContainsKey(attributeName))
                    _changes[attributeName] = value;
                else
                    _changes.Add(attributeName, value);
            }
        }

        // Fields
        public OptionSetValue Status
        {
            get { return (OptionSetValue)this[CoreFields.StatusField]; }
            set { this[CoreFields.StatusField] = value; }
        }

        public OptionSetValue StatusReason
        {
            get { return (OptionSetValue)this[CoreFields.StatusReasonField]; }
            set { this[CoreFields.StatusReasonField] = value; }
        }

        // Public methods

        public bool IsDirty()
        {
            return _changes.Count > 0;
        }

        /// <summary>
        /// Register the entity object for preoperation mode
        /// This allows plugins to use "Save()" where required
        /// and only update the target entity reference
        /// </summary>
        /// <param name="target">Target entity of the plugin</param>
        public void RegisterAsPreOperation(Entity target)
        {
            _targetEntity = target;
            _preOperation = true;
        }

        /// <summary>
        ///     Save the record - be this a Create or Update, only changes sent
        ///     will be pushed to the server.
        /// </summary>
        public void Save()
        {
            // Validate service object
            if (_service == null)
                throw new Exception("Unable to save an entity object without a service object");

            // 21.10.2015, RA:  Updated to allow for preoperation plugins
            //                  to correctly update the target entity
            if (this._preOperation && this._targetEntity != null)
            {
                foreach (var c in this._changes)
                    this._targetEntity[c.Key] = c.Value;
                return;
            }

            // Create the new Entity object
            var obj = new Entity(_logicalName);

            // Add all changes attributes;
            obj.Attributes.AddRange(_changes);

            // Check if this is an Update
            if (_entityGuid != Guid.Empty && _entityGuid != null)
            {
                // RA:  27.11.2016  -   Ensure there are changes!
                if (_changes.Count == 0)
                    return;
                // END

                obj.Id = (Guid)_entityGuid;
                _service.Update(obj);
            }
            else
            {
                // This is an Create
                _entityGuid = _service.Create(obj);
            }
            _changes = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Function to delete the current entity
        /// </summary>
        public void Delete()
        {
            // Check the id 
            if (_entityGuid != null)
                _service.Delete(_logicalName, (Guid)_entityGuid);
        }

        /// <summary>
        ///     Get multiple attributes on the current entity.
        /// </summary>
        /// <param name="attributes">String array of attributes</param>
        public EntityBase Get(string[] attributes)
        {
            // If we have no id, do nothing
            if (_entityGuid == null)
                return null;

            // Get the attributes;
            Entity entity;
            try
            {
                entity = _service.Retrieve(
                    _logicalName,
                    (Guid)_entityGuid,
                    new ColumnSet(attributes)
                );
            }
            catch
            {
                return null;
            }

            // Foreach attribute, add to the entity;
            foreach (var attribute in attributes)
                if (_values.ContainsKey(attribute))
                    _values[attribute] =
                        entity.Contains(attribute)
                            ? entity[attribute]
                            : null;
                else
                    _values.Add(
                        attribute,
                        entity.Contains(attribute)
                            ? entity[attribute]
                            : null
                    );

            // We have updated the entity.. 
            // Remove all changes relating to the fields updated
            var tmp = _changes;
            _changes = new Dictionary<string, object>();
            foreach (var change in tmp.Where(change => !attributes.Contains(change.Key)))
                _changes.Add(change);

            // Finally return this object
            return this;
        }

        /// <summary>
        ///     Get a single attribute at the current entity
        /// </summary>
        /// <param name="attribute">string of attribute to retreive</param>
        public object Get(string attribute)
        {
            Get(new[] { attribute });
            return this[attribute];
        }

        /// <summary>
        ///     Gets a record based on parameters sent
        /// </summary>
        /// <param name="id">GUID of the record</param>
        /// <param name="columnSet">Column Set - Which fields are needed?</param>
        /// <param name="logicalName">Logical Name of Entity to Retreive</param>
        /// <param name="service">Service object</param>
        /// <returns></returns>
        public static EntityBase GetRecord(Guid id, ColumnSet columnSet, string logicalName,
            IOrganizationService service)
        {
            var tmpEntity = service.Retrieve(
                logicalName,
                id,
                columnSet
            );
            return new EntityBase(tmpEntity, service);
        }

        /// <summary>
        ///     Return a copy of the record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Entity CopyRecord(Entity entity)
        {
            var obj = entity;
            obj.Id = Guid.Empty;
            obj.Attributes.Remove(entity.LogicalName + "id");

            //this._service.Create(obj);

            return obj;
        }

        public Entity CopyRecord(ITracingService tracer = null)
        {
            var obj = new Entity(_logicalName) { Id = Guid.Empty };
            obj.Attributes.AddRange(_values.ToArray());
            tracer?.Trace("COPY RECORD: No. attributes: " + obj.Attributes.Count);
            obj.Attributes.Remove(_logicalName + "id");
            return obj;
        }

        /// <summary>
        ///     Returns a delete request for the current object
        /// </summary>
        /// <returns></returns>
        public DeleteRequest DeleteRequest()
        {
            if (_entityGuid == null)
                return null;

            // return the delete request
            return new DeleteRequest
            {
                Target = GetReference()
            };
        }

        /// <summary>
        ///     Returns an update request for the current object
        /// </summary>
        /// <returns></returns>
        public UpdateRequest UpdateRequest()
        {
            if (_entityGuid == null)
                return null;

            // RA:  27.11.2016  -   Again, ensure there are changes!
            if (_changes.Count == 0)
                return null;
            // END

            // Return the update, this can then be bulk updated
            return new UpdateRequest { Target = GetEntity() };
        }

        /// <summary>
        ///     Returns a create request for the current object
        /// </summary>
        /// <returns></returns>
        public CreateRequest CreateRequest()
        {
            // Return the update, this can then be bulk updated
            return new CreateRequest { Target = GetEntity() };
        }

        /// <summary>
        ///     Bulk Create Functionality
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ts"></param>
        protected void BulkCreate(ExecuteMultipleRequest request, ITracingService ts = null)
        {
            var responseWithNoResults = (ExecuteMultipleResponse)_service.Execute(request);
            if (ts != null)
            {
                ts.Trace("Display errors");
                foreach (var responseItem in responseWithNoResults.Responses)
                    if (responseItem.Fault != null)
                        ts.Trace("A fault occurred when processing " +
                                 request.Requests[responseItem.RequestIndex].RequestName +
                                 " request, with a fault message: " + responseItem.Fault.Message);
            }
            request.Requests.Clear();
        }

        public Entity GetEntity()
        {
            // Create the new Entity object
            var obj = new Entity(_logicalName);

            // Add all changes attributes;
            obj.Attributes.AddRange(_changes);

            // Add the ID
            if (_entityGuid != null)
                obj.Id = (Guid)_entityGuid;

            return obj;
        }

        public Entity GetFullEntity()
        {
            // Create the new Entity object
            var obj = new Entity(_logicalName);

            // Add all changes attributes;
            obj.Attributes.AddRange(_values);

            // Add the ID
            if (_entityGuid != null)
                obj.Id = (Guid)_entityGuid;

            return obj;
        }

        public EntityReference GetReference()
        {
            if (Id == null)
                throw new Exception("Unable to get Reference of a non created entity");
            return new EntityReference(_logicalName, Id.Value);
        }

        public string GetLogicalName()
        {
            if (_logicalName == null)
                throw new ArgumentNullException(nameof(_logicalName));
            return _logicalName;
        }

        public static class CoreFields
        {
            public static string
                StatusField = "statecode",
                StatusReasonField = "statuscode",
                CreatedByField = "createdby",
                CreatedOnField = "createdon",
                ModifiedByField = "modifiedby",
                ModifiedOnField = "modifiedon",
                OwnerIdField = "ownerid";
        }
    }
}
