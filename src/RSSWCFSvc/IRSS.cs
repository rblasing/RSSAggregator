﻿// Copyright 2020 Richard Blasingame. All rights reserved.

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

[assembly: ContractNamespaceAttribute("http://blasingame/RSS.xsd",
   ClrNamespace = "blasingame.RSS.xsd")]


/// <summary>
/// This service was developed contract-first, with the code being generated from
/// WSDL using:
/// 
/// svcutil.exe RSS.wsdl /Language:C# /serializer:DateContractSerializer /out:IRSS.cs
/// 
/// </summary>
namespace blasingame.RSS.xsd
{
   // added WebInvoke attributes to this interface in order to support JSON bindings
   [ServiceContractAttribute(Namespace = "http://blasingame/RSS.xsd", ConfigurationName = "RSS", Name = "RSS")]
   public interface RSS
   {
      [OperationContractAttribute(Action = "http://blasingame/getTopItems", ReplyAction = "http://blasingame/getItemsResponse")]
      [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "getTopItems")]
      getItemsResponse getTopItems(getTopItemsRequest request);

      [OperationContractAttribute(Action = "http://blasingame/getItemsByRange", ReplyAction = "http://blasingame/getItemsResponse")]
      [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "getItemsByRange")]
      getItemsResponse getItemsByRange(getItemsByRangeRequest request);

      [OperationContractAttribute(Action = "http://blasingame/getItemsByKeyword", ReplyAction = "http://blasingame/getItemsResponse")]
      [WebInvoke(BodyStyle = WebMessageBodyStyle.Bare, Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "getItemsByKeyword")]
      getItemsResponse getItemsByKeyword(getItemsByKeywordRequest request);
   }


   [CollectionDataContractAttribute(Name = "ItemList",
      Namespace = "http://blasingame/RSS.xsd", ItemName = "item")]
   public class ItemList : System.Collections.Generic.List<Item>
   {
   }


   [DataContractAttribute(Name = "Item", Namespace = "http://blasingame/RSS.xsd")]
   public partial class Item
   {
      private string titleField;
      private string publisherField;
      private System.DateTime pubDateField;
      private string descriptionField;
      private System.Uri linkField;

      [DataMemberAttribute(IsRequired = true, EmitDefaultValue = false)]
      public string title
      {
         get { return this.titleField; }
         set { this.titleField = value; }
      }

      [DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 1)]
      public string publisher
      {
         get { return this.publisherField; }
         set { this.publisherField = value; }
      }

      [DataMemberAttribute(IsRequired = true, Order = 2)]
      public System.DateTime pubDate
      {
         get { return this.pubDateField; }
         set { this.pubDateField = value; }
      }

      [DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 3)]
      public string description
      {
         get { return this.descriptionField; }
         set { this.descriptionField = value; }
      }

      [DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 4)]
      public System.Uri link
      {
         get { return this.linkField; }
         set { this.linkField = value; }
      }
   }


   [MessageContractAttribute(IsWrapped = false)]
   public partial class getTopItemsRequest
   {
      [MessageBodyMemberAttribute(Name = "getTopItemsRequest",
         Namespace = "http://blasingame/RSS.xsd", Order = 0)]
      public getTopItemsRequestBody Body;

      public getTopItemsRequest()
      {
      }

      public getTopItemsRequest(getTopItemsRequestBody Body)
      {
         this.Body = Body;
      }
   }


   [DataContractAttribute(Namespace = "http://blasingame/RSS.xsd")]
   public partial class getTopItemsRequestBody
   {
      [DataMemberAttribute(Order = 0)]
      public long itemCount;

      public getTopItemsRequestBody()
      {
      }

      public getTopItemsRequestBody(long itemCount)
      {
         this.itemCount = itemCount;
      }
   }


   [MessageContractAttribute(IsWrapped = false)]
   public partial class getItemsByRangeRequest
   {
      [MessageBodyMemberAttribute(Name = "getItemsByRangeRequest",
         Namespace = "http://blasingame/RSS.xsd", Order = 0)]
      public getItemsByRangeRequestBody Body;

      public getItemsByRangeRequest()
      {
      }

      public getItemsByRangeRequest(getItemsByRangeRequestBody Body)
      {
         this.Body = Body;
      }
   }


   [DataContractAttribute(Namespace = "http://blasingame/RSS.xsd")]
   public partial class getItemsByRangeRequestBody
   {
      [DataMemberAttribute(Order = 0)]
      public System.DateTime minDateTime;

      [DataMemberAttribute(Order = 1)]
      public System.DateTime maxDateTime;

      public getItemsByRangeRequestBody()
      {
      }

      public getItemsByRangeRequestBody(System.DateTime minDateTime,
         System.DateTime maxDateTime)
      {
         this.minDateTime = minDateTime;
         this.maxDateTime = maxDateTime;
      }
   }


   [MessageContractAttribute(IsWrapped = false)]
   public partial class getItemsByKeywordRequest
   {
      [MessageBodyMemberAttribute(Name = "getItemsByKeywordRequest",
         Namespace = "http://blasingame/RSS.xsd", Order = 0)]
      public getItemsByKeywordRequestBody Body;

      public getItemsByKeywordRequest()
      {
      }

      public getItemsByKeywordRequest(getItemsByKeywordRequestBody Body)
      {
         this.Body = Body;
      }
   }


   [DataContractAttribute(Namespace = "http://blasingame/RSS.xsd")]
   public partial class getItemsByKeywordRequestBody
   {
      [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
      public string keyword;

      public getItemsByKeywordRequestBody()
      {
      }

      public getItemsByKeywordRequestBody(string keyword)
      {
         this.keyword = keyword;
      }
   }
   [DataContractAttribute(Name = "WSError", Namespace = "http://blasingame/RSS.xsd")]


   [MessageContractAttribute(IsWrapped = false)]
   public partial class getItemsResponse
   {
      [MessageBodyMemberAttribute(Name = "getItemsResponse",
         Namespace = "http://blasingame/RSS.xsd", Order = 0)]
      public getItemsResponseBody Body;

      public getItemsResponse()
      {
      }

      public getItemsResponse(getItemsResponseBody Body)
      {
         this.Body = Body;
      }
   }


   [DataContractAttribute(Namespace = "http://blasingame/RSS.xsd")]
   public partial class getItemsResponseBody
   {
      [DataMemberAttribute(EmitDefaultValue = false, Order = 0)]
      public ItemList items;

      [DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
      public WSError error;

      public getItemsResponseBody()
      {
      }

      public getItemsResponseBody(ItemList items, WSError error)
      {
         this.items = items;
         this.error = error;
      }
   }


   public partial class WSError
   {
      private WSErrorType typeField;
      private bool resubmitField;
      private string codeField;
      private string descriptionField;

      public WSError()
      {
      }

      public WSError(WSErrorType t, bool resubmit, string code, string desc)
      {
         typeField = t;
         resubmitField = resubmit;
         codeField = code;
         descriptionField = desc;
      }

      [DataMemberAttribute(IsRequired = true)]
      public WSErrorType type
      {
         get { return this.typeField; }
         set { this.typeField = value; }
      }

      [DataMemberAttribute(IsRequired = true, Order = 1)]
      public bool resubmit
      {
         get { return this.resubmitField; }
         set { this.resubmitField = value; }
      }

      [DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
      public string code
      {
         get { return this.codeField; }
         set { this.codeField = value; }
      }

      [DataMemberAttribute(EmitDefaultValue = false, Order = 3)]
      public string description
      {
         get { return this.descriptionField; }
         set { this.descriptionField = value; }
      }
   }


   [DataContractAttribute(Name = "WSErrorType", Namespace = "http://blasingame/RSS.xsd")]
   public enum WSErrorType : int
   {
      [EnumMemberAttribute()]
      Database = 0,

      [EnumMemberAttribute()]
      InvalidArgument = 1,

      [EnumMemberAttribute()]
      Security = 2,

      [EnumMemberAttribute()]
      Unavailable = 3,

      [EnumMemberAttribute()]
      Unknown = 4,
   }
}