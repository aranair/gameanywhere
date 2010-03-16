using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Amazon;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;

namespace GameAnywhere
{
    class SimpleDB
    {
        private AmazonSimpleDBClient sdb;
        private string domain;

        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleDB()
        {
            sdb = new AmazonSimpleDBClient("*", "*");
            domain = "UserAccounts";
        }

        /// <summary>
        /// Inserts item into simpleDB
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="password">user's password</param>
        /// <param name="activationKey">activation key created by md5hash using email+password</param>
        public void InsertItem(string itemId, string password, string activationKey)
        {
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!password.Equals("") && password != null);
            Debug.Assert(!activationKey.Equals("") && activationKey != null);

            PutAttributesRequest putAttributesActionOne = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId);
            List<ReplaceableAttribute> attributesOne = putAttributesActionOne.Attribute;
            attributesOne.Add(new ReplaceableAttribute().WithName("Password").WithValue(password.Trim()));
            attributesOne.Add(new ReplaceableAttribute().WithName("ActivationStatus").WithValue(0.ToString()));
            attributesOne.Add(new ReplaceableAttribute().WithName("ActivationKey").WithValue(activationKey.Trim()));
            try
            {
                sdb.PutAttributes(putAttributesActionOne);
            }
            catch(AmazonSimpleDBException ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    //ErrorCodes:
                    //NumberDomainBytesExceeded - Domain exceeds 10GB
                    //NumberDomainAttributesExceeded - Domain attributes exceed 1 million
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates item's attribute value
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="attribute">attribute to update</param>
        /// <param name="newAttributeValue">new attribute value to replace</param>
        public void UpdateAttributeValue(string itemId, string attribute, string newAttributeValue)
        {
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);
            Debug.Assert(!newAttributeValue.Equals("") && newAttributeValue != null);

            ReplaceableAttribute replaceableAttribute = new ReplaceableAttribute().WithName(attribute).WithValue(newAttributeValue).WithReplace(true);
            PutAttributesRequest replaceAction = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttribute(replaceableAttribute);
            try
            {
                sdb.PutAttributes(replaceAction);
            }
            catch (AmazonSimpleDBException ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    //ErrorCodes:
                    //NumberDomainBytesExceeded - Domain exceeds 10GB
                    Console.WriteLine("ErrorCode=" + ex.ErrorCode);
                    throw;
                }
            }
        }

        /// <summary>
        /// Checks if item exists in simpleDB
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <returns>
        /// true - item exists in simpleDB
        /// false - item does not exists in simpleDB
        /// </returns>
        public bool ItemExists(string itemId)
        {
            Debug.Assert(!itemId.Equals("") && itemId != null);

            SelectRequest request = new SelectRequest();
            request.SelectExpression = "SELECT count(*) FROM " + domain + " WHERE itemName() = '" + itemId + "'";
            try
            {
                SelectResponse selectResponse = sdb.Select(request);
                Item DomainItem = selectResponse.SelectResult.Item[0];
                string countResult = DomainItem.Attribute[0].Value;

                if (countResult.Equals("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (AmazonSimpleDBException)
            {
                throw new System.Net.WebException("Unable to connect to the internet.");
            }
        }

        /// <summary>
        /// Gets the attribute's value from given item
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="attribute">attribute</param>
        /// <returns>attribute's value</returns>
        public string GetAttribute(string itemId, string attribute)
        {
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);

            GetAttributesRequest request = new GetAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttributeName(attribute);
            try
            {
                GetAttributesResponse response = sdb.GetAttributes(request);
                GetAttributesResult result = response.GetAttributesResult;
                if (result.IsSetAttribute())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Amazon.SimpleDB.Model.Attribute att in result.Attribute)
                    {
                        if (att.IsSetValue())
                        {
                            sb.Append(att.Value).Append(",");
                        }
                    }
                    return sb.ToString().TrimEnd(',');
                }
                else
                {
                    return "";
                }
            }
            catch (AmazonSimpleDBException)
            {
                throw new System.Net.WebException("Unable to connect to the internet.");
            }
        }


        //METHODS NOT USE BUT COULD BE HELPFUL IN FUTURE
        /*
        public void DeleteItem(string itemId)
        {
            Console.WriteLine("DELETE item from UserAccounts domain.\n");
            DeleteAttributesRequest deleteItemAction = new DeleteAttributesRequest().WithDomainName(domain).WithItemName(itemId);
            sdb.DeleteAttributes(deleteItemAction);
        }

        public void AddAttributeValue(string itemId, string attribute, string attributeValue)
        {
            //Adding an attribute+value
            Console.WriteLine("ADDING Attribute+Value.\n");
            PutAttributesRequest request = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId);
            request.WithAttribute(new ReplaceableAttribute().WithName(attribute).WithValue(attributeValue));
            sdb.PutAttributes(request);
        }

        public void DeleteAttribute(string itemId, string attribute)
        {
            //Deleting an attribute
            Console.WriteLine("DELETE Attribute.\n");
            Amazon.SimpleDB.Model.Attribute deleteAttribute = new Amazon.SimpleDB.Model.Attribute().WithName(attribute);
            DeleteAttributesRequest deleteAttributeAction = new DeleteAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttribute(deleteAttribute);
            sdb.DeleteAttributes(deleteAttributeAction);
        }

        public void DeleteAttributeValue(string itemId, string attribute, string attributeValue)
        {
            // Deleting values from an attribute
            Console.WriteLine("DELETE AttributeValue.\n");
            Amazon.SimpleDB.Model.Attribute deleteValueAttribute = new Amazon.SimpleDB.Model.Attribute().WithName(attribute).WithValue(attributeValue);
            DeleteAttributesRequest deleteValueAction = new DeleteAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttribute(deleteValueAttribute);
            sdb.DeleteAttributes(deleteValueAction);
        }

        public void CreateDomain()
        {
            CreateDomainRequest createDomain = (new CreateDomainRequest()).WithDomainName(domain);
            sdb.CreateDomain(createDomain);
        }

        public void DeleteDomain()
        {
            DeleteDomainRequest deleteDomain = new DeleteDomainRequest();
            deleteDomain.DomainName = domain;
            sdb.DeleteDomain(deleteDomain);
        }

        public void ListDomains()
        {
            // Listing domains
            ListDomainsResponse sdbListDomainsResponse = sdb.ListDomains(new ListDomainsRequest());
            if (sdbListDomainsResponse.IsSetListDomainsResult())
            {
                ListDomainsResult listDomainsResult = sdbListDomainsResponse.ListDomainsResult;
                Console.WriteLine("List of domains:\n");
                foreach (String domain in listDomainsResult.DomainName)
                {
                    Console.WriteLine("  " + domain);
                }
            }
            Console.WriteLine();
        }
        */
    }
}
