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
    /// <summary>
    /// Provides methods to access and update Amazon Web Services (AWS) SimpleDB.
    /// </summary>
    /// <remarks>This class requires the AWSSDK DLL.</remarks>
    class SimpleDB
    {
        #region Data Members
        /// <summary>
        /// AWS Access Key.
        /// </summary>
        private string accessKey;

        /// <summary>
        /// AWS Secret Access Key.
        /// </summary>
        private string secretAccessKey;

        /// <summary>
        /// AWS SimpleDB client.
        /// </summary>
        private AmazonSimpleDBClient sdb;

        /// <summary>
        /// Domain in AWS SimpleDB.
        /// </summary>
        private string domain;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the data members.
        /// </summary>
        public SimpleDB()
        {
            this.accessKey = "*";
            this.secretAccessKey = "*";
            this.sdb = new AmazonSimpleDBClient(accessKey, secretAccessKey);
            this.domain = "UserAccounts";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Inserts an item into DB.
        /// </summary>
        /// <param name="itemId">Item id.</param>
        /// <param name="password">User password.</param>
        /// <param name="activationKey">Activation key.</param>
        /// <exception cref="AmazonSimpleDBException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public void InsertItem(string itemId, string password, string activationKey)
        {
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!password.Equals("") && password != null);
            Debug.Assert(!activationKey.Equals("") && activationKey != null);

            if(String.IsNullOrEmpty(itemId))
                throw new ArgumentException("Parameter cannot be empty/null", "itemId");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Parameter cannot be empty/null", "password");
            if (String.IsNullOrEmpty(activationKey))
                throw new ArgumentException("Parameter cannot be empty/null", "activationKey");

            //Setup request
            PutAttributesRequest putAttributesActionOne = new PutAttributesRequest().WithDomainName(domain).WithItemName(itemId);
            List<ReplaceableAttribute> attributesOne = putAttributesActionOne.Attribute;
            attributesOne.Add(new ReplaceableAttribute().WithName("Password").WithValue(password.Trim()));
            attributesOne.Add(new ReplaceableAttribute().WithName("ActivationStatus").WithValue("0"));
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
                    //ConnectionFailureException thrown
                    throw new ConnectionFailureException("Unable to connect to web server.");
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
        /// Updates an item's attribute value.
        /// </summary>
        /// <param name="itemId">Item id.</param>
        /// <param name="attribute">Attribute name.</param>
        /// <param name="newAttributeValue">Attribute value.</param>
        /// <exception cref="AmazonSimpleDBException"></exception>
        /// <exception cref="ConnectionFailureException"></exception>
        public void UpdateAttributeValue(string itemId, string attribute, string newAttributeValue)
        {
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);
            Debug.Assert(!newAttributeValue.Equals("") && newAttributeValue != null);

            if (String.IsNullOrEmpty(itemId))
                throw new ArgumentException("Parameter cannot be empty/null", "itemId");
            if (String.IsNullOrEmpty(attribute))
                throw new ArgumentException("Parameter cannot be empty/null", "attribute");
            if (String.IsNullOrEmpty(newAttributeValue))
                throw new ArgumentException("Parameter cannot be empty/null", "newAttributeValue");

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
                    //ConnectionFailureException thrown
                    throw new ConnectionFailureException("Unable to connect to web server.");
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
        /// Checks if item exists in DB.
        /// </summary>
        /// <param name="itemId">Item id.</param>
        /// <returns>
        /// True - Item exists in DB.
        /// false - Item does not exists in DB.
        /// </returns>
        /// <exception cref="ConnectionFailureException"></exception>
        public bool ItemExists(string itemId)
        {
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);

            if (String.IsNullOrEmpty(itemId))
                throw new ArgumentException("Parameter cannot be empty/null", "itemId");

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
                //ConnectionFailureException thrown
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }

        /// <summary>
        /// Gets the attribute's value from an item.
        /// </summary>
        /// <param name="itemId">Item id.</param>
        /// <param name="attribute">Attribute name.</param>
        /// <returns>Attribute's value.</returns>
        /// <exception cref="ConnectionFailureException"></exception>
        public string GetAttribute(string itemId, string attribute)
        {
            //Pre-conditions
            Debug.Assert(!itemId.Equals("") && itemId != null);
            Debug.Assert(!attribute.Equals("") && attribute != null);

            if (String.IsNullOrEmpty(itemId))
                throw new ArgumentException("Parameter cannot be empty/null", "itemId");
            if (String.IsNullOrEmpty(attribute))
                throw new ArgumentException("Parameter cannot be empty/null", "attribute");

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
                //ConnectionFailureException thrown
                throw new ConnectionFailureException("Unable to connect to web server.");
            }
        }
        #endregion
    }
}
