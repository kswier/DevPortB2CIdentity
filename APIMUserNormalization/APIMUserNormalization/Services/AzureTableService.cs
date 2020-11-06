using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace APIMUserNormalization.Services
{
    public class AzureTableService
    {
        string storageConn = "";
        string storageTable = "";
        public AzureTableService(string storage, string table)
        {
            storageConn = storage;
            storageTable = table;
        }

        public void WriteSuccessEnablement(string apim, string email, string props, string pwd, bool enabled)
        {
            CloudStorageAccount storageAcc = CloudStorageAccount.Parse(storageConn);
            CloudTableClient tblclient = storageAcc.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tblclient.GetTableReference(storageTable);

            APIMUser au = new APIMUser(apim, email, props, pwd, enabled);

            TableOperation insertOperation = TableOperation.InsertOrMerge(au);
            var results = table.Execute(insertOperation);

            return;
        }
    }


    internal class APIMUser : TableEntity
    {
        public string Properties { get; set; }
        public string Password { get; set; }
        public bool EnabledB2C { get; set; }

        public APIMUser(string apim, string email, string props, string pwd, bool inB2C)
        {
            PartitionKey = apim;
            RowKey = email;
            EnabledB2C = inB2C;
            Properties = props;
            Password = pwd;
        }


    }
}
