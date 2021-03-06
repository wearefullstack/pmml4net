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
using System.Xml;
using System.Globalization;

namespace pmml4net
{
	/// <summary>
	/// Description of SimpleRule.
	/// </summary>
	public class SimpleRule : Rule
	{
		private string fid;
		private string foperator;
		private string fvalue;
		private Predicate predicate;
		private string fscore;
		
		private IList<ScoreDistribution> scoreDistributions = new List<ScoreDistribution>();
		
		/// <summary>
		/// the condition upon which the rule fires. For more details on PREDICATE 
		/// see the section on predicates in TreeModel. This explains how predicates 
		/// are described and evaluated and how missing values are handled.
		/// </summary>
		public Predicate Predicate { get { return predicate; } set { predicate = value; } }
		
		/// <summary>
		/// This attribute of the SimplePredicate element is the name attribute of a MiningField or a DerivedField from
		/// TransformationDictionary or LocalTransformations. For Predicates contained within the Segment elements of 
		/// MiningModels that employ Segmentation's modelChain MULTIPLE-MODEL-METHOD approach, field can also refer to 
		/// an OutputField from an earlier Segment.
		/// </summary>
		public string Id { get { return fid; } set { fid = value; } }
		
		/// <summary>
		/// This attribute of SimplePredicate is one of the six pre-defined comparison operators.
		/// <code>
		/// Operator	Math Symbol
		/// equal	=
		/// notEqual	≠
		/// lessThan	&lt;
		/// lessOrEqual	≤
		/// greaterThan	>
		/// greaterOrEqual	≥
		/// </code>
		/// </summary>
		public string Operator { get { return foperator; } set { foperator = value; } }
		
		/// <summary>
		/// This attribute of <code>SimplePredicate</code> element is the information to evaluate / compare against.
		/// </summary>
		public string Value { get { return fvalue; } set { fvalue = value; } }
		
		/// <summary>
		/// The predicted value when the rule fires.
		/// </summary>
		public string Score { get { return fscore; } set { fscore = value; } }
		
		/// <summary>
		/// siblings of this node
		/// </summary>
		public IList<ScoreDistribution> ScoreDistributions { get { return scoreDistributions; } set { scoreDistributions = value; } }
		
		/// <summary>
		/// Load Node from XmlNode
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static SimpleRule loadFromXmlNode(XmlNode node)
		{
			SimpleRule root = new SimpleRule();
			
			// TODO : Add extention reading
			
			if (node.Attributes["id"] == null)
				throw new PmmlException("Attribute 'id' is required in 'SimpleRule'.");
			root.fid = node.Attributes["id"].Value;
			
			/*if (node.Attributes["operator"] == null)
				throw new PmmlException("Attribute 'operator' is required in 'SimplePredicate'.");
			root.foperator = node.Attributes["operator"].Value;
			
			if (!("isMissing".Equals(root.foperator) || "isNotMissing".Equals(root.foperator)))
			{
				if (node.Attributes["value"] == null)
					throw new PmmlException(string.Format("Attribute 'value' is required in 'SimplePredicate' if 'operator' is '{0}'.", root.foperator));
				root.fvalue = node.Attributes["value"].Value;
			}*/
			
			root.fscore = node.Attributes["score"].Value;
			
			foreach(XmlNode item in node.ChildNodes)
			{
				if ("extension".Equals(item.Name.ToLowerInvariant()))
				{
					// TODO : implement extension
					//root.Nodes.Add(Node.loadFromXmlNode(item));
				}
				else if ("simplepredicate".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = SimplePredicate.loadFromXmlNode(item);
				}
				else if ("compoundpredicate".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = CompoundPredicate.loadFromXmlNode(item);
				}
				else if ("scoredistribution".Equals(item.Name.ToLowerInvariant()))
				{
					root.ScoreDistributions.Add(ScoreDistribution.loadFromXmlNode(item));
				}
				else
					throw new NotImplementedException();
			}
			
			return root;
		}
		
		/// <summary>
		/// Test predicate
		/// </summary>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override PredicateResult Evaluate(Dictionary<string, object> dict)
		{
			return this.Predicate.Evaluate(dict);
			/*
			// Manage 'isMissing' operator
			if ("ismissing".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(!dict.ContainsKey(field));
			
			else if ("isnotmissing".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(dict.ContainsKey(field));
			
			// Manage surrogate mode
			if (!dict.ContainsKey(field))
				return PredicateResult.Unknown;
			
			object var_test = dict[field];
			
			//Console.WriteLine("Test <field={0}[" + var_test + "]> <operator={1}> <value={2}>", field, foperator, fvalue);
			
			if ("equal".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(var_test.Equals(fvalue));
			
			else if ("notequal".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(!var_test.Equals(fvalue));
			
			decimal var_test_double = Convert.ToDecimal(dict[field], CultureInfo.InvariantCulture);
			decimal ref_double = Convert.ToDecimal(fvalue, CultureInfo.InvariantCulture);
			
			
			if ("lessthan".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(var_test_double < ref_double);
			
			else if ("lessorequal".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(var_test_double <= ref_double);
			
			else if ("greaterthan".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(var_test_double > ref_double);
			
			else if ("greaterorequal".Equals(foperator.Trim().ToLowerInvariant()))
				return ToPredicateResult(var_test_double >= ref_double);
			
			else
				throw new PmmlException();*/
			//throw new NotImplementedException();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void save(XmlWriter writer)
		{
			writer.WriteStartElement("SimplePredicate");
			
			if (!string.IsNullOrEmpty(this.Id))
				writer.WriteAttributeString("id", this.Id);
			
			writer.WriteAttributeString("operator", this.Operator);
			if (!string.IsNullOrEmpty(this.Value))
				writer.WriteAttributeString("value", this.Value);
			
			writer.WriteEndElement();
		}
	}
}
