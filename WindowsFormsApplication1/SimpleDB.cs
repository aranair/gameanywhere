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
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!password.Equals("") && password != null);
            Debug.Assert(!activationKey.Equals("") && activationKey != null);

            //Setup request
            PutAttributesRequest putAttributesActionOne = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId);
            List<ReplaceableAttribute> attributesOne = putAttributesActionOne.Attribute;
            attributesOne.Add(new ReplaceableAttribute().WithName("Password").WithValue(password.Trim()));
            attributesOne.Add(new ReplaceableAttribute().WithName("ActivationStatus").WithValue(0.ToString()));
            attributesOne.Add(new ReplaceableAttribute().WithName("ActivationKey").WithValue(activationKey.Trim()));
            try
            {
                //Send request and get response
                sdb.PutAttributes(putAttributesActionOne);
            }
            catch(AmazonSimpleDBException ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    //System.Net.WebException thrown
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    //ErrorCodes:
                    //NumberDomainBytesExceeded - Domain exceeds 10GB
                    //NumberDomainAttributesExceeded - Domain attributes exceed 1 million
                    //AmazonSimpleDBException thrown
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
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);
            Debug.Assert(!newAttributeValue.Equals("") && newAttributeValue != null);

            //Setup request
            ReplaceableAttribute replaceableAttribute = new ReplaceableAttribute().WithName(attribute).WithValue(newAttributeValue).WithReplace(true);
            PutAttributesRequest replaceAction = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttribute(replaceableAttribute);
            try
            {
                //Send request and get response
                sdb.PutAttributes(replaceAction);
            }
            catch (AmazonSimpleDBException ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(System.Net.WebException)))
                {
                    //System.Net.WebException thrown
                    Console.WriteLine("No internet connection.");
                    throw new System.Net.WebException("Unable to connect to the internet.");
                }
                else
                {
                    //ErrorCodes:
                    //NumberDomainBytesExceeded - Domain exceeds 10GB
                    //AmazonSimpleDBException thrown
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
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);

            //Setup request - using select statement
            SelectRequest request = new SelectRequest();
            request.SelectExpression = "SELECT count(*) FROM " + domain + " WHERE itemName() = '" + itemId + "'";
            try
            {
                //Send request and get response
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
                //System.Net.WebException thrown
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
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);

            //Setup request
            GetAttributesRequest request = new GetAttributesRequest().WithDomainName(domain).WithItemName(itemId).WithAttributeName(attribute);
            try
            {
                //Send request and get response
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
                //System.Net.WebException thrown
                throw new System.Net.WebException("Unable to connect to the internet.");
            }
        }
    }
}
