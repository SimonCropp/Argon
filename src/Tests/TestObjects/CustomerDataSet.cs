// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantAttributeSuffix
namespace TestObjects;

/// <summary>
///Represents a strongly typed in-memory cache of data.
///</summary>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.ComponentModel.ToolboxItem(true)]
[global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedDataSetSchema")]
[global::System.Xml.Serialization.XmlRootAttribute("CustomerDataSet")]
[System.ComponentModel.Design.HelpKeywordAttribute("vs.data.DataSet")]
public class CustomerDataSet : System.Data.DataSet
{
    CustomersDataTable tableCustomers;

    System.Data.SchemaSerializationMode _schemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public CustomerDataSet()
    {
        BeginInit();
        InitClass();
        var schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(SchemaChanged);
        base.Tables.CollectionChanged += schemaChangedHandler;
        base.Relations.CollectionChanged += schemaChangedHandler;
        EndInit();
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected CustomerDataSet(SerializationInfo info, StreamingContext context) :
        base(info, context, false)
    {
        if (IsBinarySerialized(info, context) == true)
        {
            InitVars(false);
            var schemaChangedHandler1 = new System.ComponentModel.CollectionChangeEventHandler(SchemaChanged);
            Tables.CollectionChanged += schemaChangedHandler1;
            Relations.CollectionChanged += schemaChangedHandler1;
            return;
        }
        var strSchema = (string)info.GetValue("XmlSchema", typeof(string));
        if (DetermineSchemaSerializationMode(info, context) == System.Data.SchemaSerializationMode.IncludeSchema)
        {
            var ds = new System.Data.DataSet();
            ds.ReadXmlSchema(new System.Xml.XmlTextReader(new StringReader(strSchema)));
            if (ds.Tables["Customers"] != null)
            {
                base.Tables.Add(new CustomersDataTable(ds.Tables["Customers"]));
            }
            DataSetName = ds.DataSetName;
            Prefix = ds.Prefix;
            Namespace = ds.Namespace;
            Locale = ds.Locale;
            CaseSensitive = ds.CaseSensitive;
            EnforceConstraints = ds.EnforceConstraints;
            Merge(ds, false, System.Data.MissingSchemaAction.Add);
            InitVars();
        }
        else
        {
            ReadXmlSchema(new System.Xml.XmlTextReader(new StringReader(strSchema)));
        }
        GetSerializationData(info, context);
        var schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(SchemaChanged);
        base.Tables.CollectionChanged += schemaChangedHandler;
        Relations.CollectionChanged += schemaChangedHandler;
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
    public CustomersDataTable Customers => tableCustomers;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [System.ComponentModel.BrowsableAttribute(true)]
    [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public override System.Data.SchemaSerializationMode SchemaSerializationMode
    {
        get => _schemaSerializationMode;
        set => _schemaSerializationMode = value;
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public new System.Data.DataTableCollection Tables => base.Tables;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public new System.Data.DataRelationCollection Relations => base.Relations;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override void InitializeDerivedDataSet()
    {
        BeginInit();
        InitClass();
        EndInit();
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public override System.Data.DataSet Clone()
    {
        var cln = (CustomerDataSet)base.Clone();
        cln.InitVars();
        cln.SchemaSerializationMode = SchemaSerializationMode;
        return cln;
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override bool ShouldSerializeTables() =>
        false;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override bool ShouldSerializeRelations() =>
        false;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override void ReadXmlSerializable(System.Xml.XmlReader reader)
    {
        if (DetermineSchemaSerializationMode(reader) == System.Data.SchemaSerializationMode.IncludeSchema)
        {
            Reset();
            var ds = new System.Data.DataSet();
            ds.ReadXml(reader);
            if (ds.Tables["Customers"] != null)
            {
                base.Tables.Add(new CustomersDataTable(ds.Tables["Customers"]));
            }
            DataSetName = ds.DataSetName;
            Prefix = ds.Prefix;
            Namespace = ds.Namespace;
            Locale = ds.Locale;
            CaseSensitive = ds.CaseSensitive;
            EnforceConstraints = ds.EnforceConstraints;
            Merge(ds, false, System.Data.MissingSchemaAction.Add);
            InitVars();
        }
        else
        {
            ReadXml(reader);
            InitVars();
        }
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override global::System.Xml.Schema.XmlSchema GetSchemaSerializable()
    {
        var stream = new MemoryStream();
        WriteXmlSchema(new System.Xml.XmlTextWriter(stream, null));
        stream.Position = 0;
        return System.Xml.Schema.XmlSchema.Read(new System.Xml.XmlTextReader(stream), null);
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    internal void InitVars() =>
        InitVars(true);

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    internal void InitVars(bool initTable)
    {
        tableCustomers = (CustomersDataTable)base.Tables["Customers"];
        if (initTable == true)
        {
            tableCustomers?.InitVars();
        }
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    void InitClass()
    {
        DataSetName = "CustomerDataSet";
        Prefix = "";
        EnforceConstraints = true;
        SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
        tableCustomers = new();
        base.Tables.Add(tableCustomers);
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    bool ShouldSerializeCustomers() =>
        false;

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    void SchemaChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
    {
        if (e.Action == System.ComponentModel.CollectionChangeAction.Remove)
        {
            InitVars();
        }
    }

    [DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedDataSetSchema(global::System.Xml.Schema.XmlSchemaSet xs)
    {
        var ds = new CustomerDataSet();
        var type = new global::System.Xml.Schema.XmlSchemaComplexType();
        var sequence = new global::System.Xml.Schema.XmlSchemaSequence();
        var any = new global::System.Xml.Schema.XmlSchemaAny
        {
            Namespace = ds.Namespace
        };
        sequence.Items.Add(any);
        type.Particle = sequence;
        var dsSchema = ds.GetSchemaSerializable();
        if (xs.Contains(dsSchema.TargetNamespace))
        {
            var s1 = new MemoryStream();
            var s2 = new MemoryStream();
            try
            {
                dsSchema.Write(s1);
                for (var schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext();)
                {
                    var schema = (global::System.Xml.Schema.XmlSchema)schemas.Current;
                    s2.SetLength(0);
                    schema.Write(s2);
                    if (s1.Length == s2.Length)
                    {
                        s1.Position = 0;
                        s2.Position = 0;
                        for (; s1.Position != s1.Length
                               && s1.ReadByte() == s2.ReadByte();)
                        {
                            ;
                        }
                        if (s1.Position == s1.Length)
                        {
                            return type;
                        }
                    }
                }
            }
            finally
            {
                s1.Close();

                s2.Close();
            }
        }
        xs.Add(dsSchema);
        return type;
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public delegate void CustomersRowChangeEventHandler(object sender, CustomersRowChangeEvent e);

    /// <summary>
    ///Represents the strongly named DataTable class.
    ///</summary>
    [Serializable()]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedTableSchema")]
    public class CustomersDataTable : System.Data.DataTable, IEnumerable
    {
        System.Data.DataColumn columnCustomerID;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersDataTable()
        {
            TableName = "Customers";
            BeginInit();
            InitClass();
            EndInit();
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal CustomersDataTable(System.Data.DataTable table)
        {
            TableName = table.TableName;
            if (table.CaseSensitive != table.DataSet.CaseSensitive)
            {
                CaseSensitive = table.CaseSensitive;
            }
            if (table.Locale.ToString() != table.DataSet.Locale.ToString())
            {
                Locale = table.Locale;
            }
            if (table.Namespace != table.DataSet.Namespace)
            {
                Namespace = table.Namespace;
            }
            Prefix = table.Prefix;
            MinimumCapacity = table.MinimumCapacity;
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected CustomersDataTable(SerializationInfo info, StreamingContext context) :
            base(info, context) =>
            InitVars();

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public System.Data.DataColumn CustomerIDColumn => columnCustomerID;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        [System.ComponentModel.Browsable(false)]
        public int Count => Rows.Count;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersRow this[int index] => (CustomersRow)Rows[index];

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public event CustomersRowChangeEventHandler CustomersRowChanging;

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public event CustomersRowChangeEventHandler CustomersRowChanged;

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public event CustomersRowChangeEventHandler CustomersRowDeleting;

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public event CustomersRowChangeEventHandler CustomersRowDeleted;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public void AddCustomersRow(CustomersRow row) =>
            Rows.Add(row);

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersRow AddCustomersRow(string CustomerID)
        {
            var rowCustomersRow = (CustomersRow)NewRow();
            var columnValuesArray = new object[]
            {
                CustomerID
            };
            rowCustomersRow.ItemArray = columnValuesArray;
            Rows.Add(rowCustomersRow);
            return rowCustomersRow;
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public virtual IEnumerator GetEnumerator() =>
            Rows.GetEnumerator();

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public override System.Data.DataTable Clone()
        {
            var cln = (CustomersDataTable)base.Clone();
            cln.InitVars();
            return cln;
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override System.Data.DataTable CreateInstance() =>
            new CustomersDataTable();

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal void InitVars() =>
            columnCustomerID = Columns["CustomerID"];

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        void InitClass()
        {
            columnCustomerID = new("CustomerID", typeof(string), null, System.Data.MappingType.Element);
            Columns.Add(columnCustomerID);
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersRow NewCustomersRow() =>
            (CustomersRow)NewRow();

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override System.Data.DataRow NewRowFromBuilder(System.Data.DataRowBuilder builder) =>
            new CustomersRow(builder);

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override Type GetRowType() =>
            typeof(CustomersRow);

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void OnRowChanged(System.Data.DataRowChangeEventArgs e)
        {
            base.OnRowChanged(e);
            CustomersRowChanged?.Invoke(this, new((CustomersRow)e.Row, e.Action));
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void OnRowChanging(System.Data.DataRowChangeEventArgs e)
        {
            base.OnRowChanging(e);
            CustomersRowChanging?.Invoke(this, new((CustomersRow)e.Row, e.Action));
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void OnRowDeleted(System.Data.DataRowChangeEventArgs e)
        {
            base.OnRowDeleted(e);
            CustomersRowDeleted?.Invoke(this, new((CustomersRow)e.Row, e.Action));
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void OnRowDeleting(System.Data.DataRowChangeEventArgs e)
        {
            base.OnRowDeleting(e);
            CustomersRowDeleting?.Invoke(this, new((CustomersRow)e.Row, e.Action));
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public void RemoveCustomersRow(CustomersRow row) =>
            Rows.Remove(row);

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedTableSchema(global::System.Xml.Schema.XmlSchemaSet xs)
        {
            var type = new global::System.Xml.Schema.XmlSchemaComplexType();
            var sequence = new global::System.Xml.Schema.XmlSchemaSequence();
            var ds = new CustomerDataSet();
            var any1 = new global::System.Xml.Schema.XmlSchemaAny
            {
                Namespace = "http://www.w3.org/2001/XMLSchema",
                MinOccurs = new(0),
                MaxOccurs = decimal.MaxValue,
                ProcessContents = System.Xml.Schema.XmlSchemaContentProcessing.Lax
            };
            sequence.Items.Add(any1);
            var any2 = new global::System.Xml.Schema.XmlSchemaAny
            {
                Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1",
                MinOccurs = new(1),
                ProcessContents = System.Xml.Schema.XmlSchemaContentProcessing.Lax
            };
            sequence.Items.Add(any2);
            var attribute1 = new global::System.Xml.Schema.XmlSchemaAttribute
            {
                Name = "namespace",
                FixedValue = ds.Namespace
            };
            type.Attributes.Add(attribute1);
            var attribute2 = new global::System.Xml.Schema.XmlSchemaAttribute
            {
                Name = "tableTypeName",
                FixedValue = "CustomersDataTable"
            };
            type.Attributes.Add(attribute2);
            type.Particle = sequence;
            var dsSchema = ds.GetSchemaSerializable();
            if (xs.Contains(dsSchema.TargetNamespace))
            {
                var s1 = new MemoryStream();
                var s2 = new MemoryStream();
                try
                {
                    dsSchema.Write(s1);
                    for (var schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext();)
                    {
                        var schema = (global::System.Xml.Schema.XmlSchema)schemas.Current;
                        s2.SetLength(0);
                        schema.Write(s2);
                        if (s1.Length == s2.Length)
                        {
                            s1.Position = 0;
                            s2.Position = 0;
                            for (; s1.Position != s1.Length
                                   && s1.ReadByte() == s2.ReadByte();)
                            {
                            }
                            if (s1.Position == s1.Length)
                            {
                                return type;
                            }
                        }
                    }
                }
                finally
                {
                    s1.Close();
                    s2.Close();
                }
            }
            xs.Add(dsSchema);
            return type;
        }
    }

    /// <summary>
    ///Represents strongly named DataRow class.
    ///</summary>
    public class CustomersRow : System.Data.DataRow
    {
        CustomersDataTable tableCustomers;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal CustomersRow(System.Data.DataRowBuilder rb) :
            base(rb) =>
            tableCustomers = (CustomersDataTable)Table;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public string CustomerID
        {
            get
            {
                try
                {
                    return (string)this[tableCustomers.CustomerIDColumn];
                }
                catch (InvalidCastException e)
                {
                    throw new System.Data.StrongTypingException("The value for column \'CustomerID\' in table \'Customers\' is DBNull.", e);
                }
            }
            set => this[tableCustomers.CustomerIDColumn] = value;
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public bool IsCustomerIDNull() =>
            IsNull(tableCustomers.CustomerIDColumn);

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public void SetCustomerIDNull() =>
            this[tableCustomers.CustomerIDColumn] = Convert.DBNull;
    }

    /// <summary>
    ///Row event argument class
    ///</summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public class CustomersRowChangeEvent : EventArgs
    {
        CustomersRow eventRow;

        System.Data.DataRowAction eventAction;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersRowChangeEvent(CustomersRow row, System.Data.DataRowAction action)
        {
            eventRow = row;
            eventAction = action;
        }

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public CustomersRow Row => eventRow;

        [DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public System.Data.DataRowAction Action => eventAction;
    }
}