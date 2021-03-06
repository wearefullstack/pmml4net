﻿/*
pmml4net - easy lib to read and consume tree model in PMML file
Copyright (C) 2013  Damien Carol <damien.carol@gmail.com>

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Library General Public
License as published by the Free Software Foundation; either
version 2 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Library General Public License for more details.

You should have received a copy of the GNU Library General Public
License along with this library; if not, write to the
Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace pmml4net
{
	/// <summary>
	/// Description of Pmml.
	/// </summary>
	public class Pmml
	{
		private Header header = new Header();
		private DataDictionary dataDictionary = new DataDictionary();
		private IList<ModelElement> models = new List<ModelElement>();
		
		/// <summary>
		/// Data dictionary.
		/// </summary>
		public DataDictionary DataDictionary { get { return dataDictionary; } set { dataDictionary = value; } }
		
		/// <summary>
		/// Model in pmml file.
		/// </summary>
		public IList<ModelElement> Models 
		{ 
			get { return models; }
		}
		
		/// <summary>
		/// Save pmml file
		/// </summary>
		/// <param name="path">Path of the PMML file</param>
		public void save(string path)
		{
			FileInfo info = new FileInfo(path);
			save(info);
		}
		
		/// <summary>
		/// Save pmml file
		/// </summary>
		/// <param name="info">Informations about the PMML file to read></param>
		public void save(FileInfo info)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineHandling = NewLineHandling.Entitize;
			
			// Write to file
			using (XmlWriter writer = XmlWriter.Create(info.FullName, settings))
				save(writer);
		}
		
		/// <summary>
		/// Save pmml file
		/// </summary>
		/// <param name="writer">Xml PMML file to read></param>
		public void save(XmlWriter writer)
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("PMML", "http://www.dmg.org/PMML-4_1");
			
			writer.WriteAttributeString("version", "4.1");
			
			// Save header
			this.header.save(writer);
			
			// Save data dictionnary
			this.dataDictionary.save(writer);
			
			foreach (ModelElement model in this.models)
			{
				model.save(writer);
			}
			
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}
		
		/// <summary>
		/// Load pmml file
		/// </summary>
		/// <param name="path">Path of the PMML file</param>
		public static Pmml loadModels(string path)
		{
			FileInfo info = new FileInfo(path);
			return loadModels(info);
		}
		
		/// <summary>
		/// Load pmml file
		/// </summary>
		/// <param name="info">Informations about the PMML file to read></param>
		public static Pmml loadModels(FileInfo info)
		{
			XmlDocument xml = new XmlDocument();
			xml.Load(info.FullName);
			return loadModels(xml);
		}
		
		/// <summary>
		/// Load pmml file
		/// </summary>
		/// <param name="xml">Xml PMML file to read></param>
		public static Pmml loadModels(XmlDocument xml)
		{
			Pmml pmml = new Pmml();
			pmml.models = new List<ModelElement>();
			
			foreach (XmlNode root in xml.ChildNodes)
			{
				if (root is XmlElement)
				{
					foreach (XmlNode child in root.ChildNodes)
					{
						if (child.Name.Equals("DataDictionary"))
						{
							pmml.DataDictionary = DataDictionary.loadFromXmlNode(child);
						}
						else if (child.Name.Equals("RuleSetModel"))
						{
							pmml.models.Add(RuleSetModel.loadFromXmlNode(child));
						}
						else if (child.Name.Equals("TreeModel"))
						{
							pmml.models.Add(TreeModel.loadFromXmlNode(child));
						}
						else if (child.Name.Equals("MiningModel"))
						{
							pmml.models.Add(MiningModel.loadFromXmlNode(child));
						}
					}
				}
			}
			
			return pmml;
		}
		
		/// <summary>
		/// Get a model by it's name.
		/// </summary>
		/// <param name="name">name of the model</param>
		/// <returns></returns>
		public ModelElement getByName(string name)
		{
			foreach (ModelElement item in models)
				if (name.Equals(item.ModelName))
					return item;
			
			return null;
		}
	}
}
