using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CRMwithAD.Module.BusinessObjects.InventoryManagement
{
    [DefaultClassOptions]
   public class Inventory : BaseObject
    {
        public Inventory(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

        }
        private string _TotalRam;
        public string TotalRam
        {
            get { return _TotalRam; }
            set { SetPropertyValue<string>(nameof(TotalRam), ref _TotalRam, value); }
        }
        private string _TotalStorage;
        public string TotalStorage
        {
            get { return _TotalStorage; }
            set { SetPropertyValue<string>(nameof(TotalStorage), ref _TotalStorage, value); }
        }

        private string _TotalProcessorCores;
        public string TotalProcessorCores
        {
            get { return TotalProcessorCores; }
            set { SetPropertyValue<string>(nameof(TotalProcessorCores), ref _TotalProcessorCores, value); }
        }
        private string _TotalServers;
        public string TotalServers
        {
            get { return _TotalServers; }
            set { SetPropertyValue<string>(nameof(TotalServers), ref _TotalServers, value); }
        }
    }

}