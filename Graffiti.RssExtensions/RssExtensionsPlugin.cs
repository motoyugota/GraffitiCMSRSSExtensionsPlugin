/* ****************************************************************************
 *
 * Copyright (c) Nexus Technologies, LLC. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found at:
 * 
 * http://graffitirssextension.codeplex.com/license
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graffiti.Core;
using System.Collections.Specialized;

namespace Graffiti.RssExtensions
{
	public class RssExtensionsPlugin : GraffitiEvent
	{
		private readonly string categoryFieldName = "Custom RSS Categories";

		public bool EnableRssCategories { get; set; }

		public override bool IsEditable
		{
			get { return true; }
		}

		public override string Name
		{
			get { return "RSS Extensions Plugin"; }
		}

		public override string Description
		{
			get { return "Extends Graffiti CMS with advanced RSS Feed options"; }
		}

		public override void Init(GraffitiApplication ga)
		{
			ga.RssItem += new RssPostEventHandler(ga_RssItem);
		}

		void ga_RssItem(System.Xml.XmlTextWriter writer, PostEventArgs e)
		{
			if (EnableRssCategories)
			{
				string categories = e.Post.Custom(categoryFieldName);
				if (categories == null || categories.Length == 0)
				{
					return;
				}
				string[] categoryItems = categories.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string category in categoryItems)
				{
					string[] categoryParts = category.Split(new string[] { "$" }, StringSplitOptions.RemoveEmptyEntries);
					writer.WriteStartElement("category");
					if (categoryParts.Length == 2)
					{
						writer.WriteAttributeString("domain", categoryParts[1]);
					}
					writer.WriteString(categoryParts[0]);
					writer.WriteEndElement();
				}
			}
		}

		protected override System.Collections.Specialized.NameValueCollection DataAsNameValueCollection()
		{
			NameValueCollection nvc = new NameValueCollection();
			nvc["enableRssCategories"] = EnableRssCategories.ToString();

			return nvc;
		}

		protected override FormElementCollection AddFormElements()
		{
			FormElementCollection fec = new FormElementCollection();
			fec.Add(new CheckFormElement("enableRssCategories", "Enable RSS Categories", "Allows you to specify custom &lt;category&gt; elements on individual posts", false));

			return fec;
		}

		public override StatusType SetValues(System.Web.HttpContext context, System.Collections.Specialized.NameValueCollection nvc)
		{
			EnableRssCategories = ConvertStringToBool(nvc["enableRssCategories"]);

			if (EnableRssCategories)
			{
				SetUpRssCategories();
			}

			return StatusType.Success;
		}

		private bool ConvertStringToBool(string checkValue)
		{
			if (string.IsNullOrEmpty(checkValue))
				return false;
			else if (checkValue == "checked" || checkValue == "on")
				return true;
			else
				return bool.Parse(checkValue);
		}

		private void SetUpRssCategories()
		{
			bool customFieldExists = false;
			CustomFormSettings cfs = CustomFormSettings.Get();
			if (cfs.Fields != null && cfs.Fields.Count > 0)
			{
				foreach (CustomField cf in cfs.Fields)
				{
					if (Util.AreEqualIgnoreCase(categoryFieldName, cf.Name))
					{
						customFieldExists = true;
						break;
					}
				}
			}

			if (!customFieldExists)
			{
				CustomField nfield = new CustomField();
				nfield.Name = categoryFieldName;
				nfield.Description = "Custom Categories you want to included in your RSS feed for your blog post.  Enter each on a new line and each item will be entered in a separate <category> element.  If you want to set a domain on the category element, add a dollar sign ($) after the category and then enter the domain value.";
				nfield.Enabled = true;
				nfield.Id = Guid.NewGuid();
				nfield.FieldType = FieldType.TextArea;

				cfs.Name = "-1";
				cfs.Add(nfield);
				cfs.Save();
			}
		}
	}
}
