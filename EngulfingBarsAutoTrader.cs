#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	
		
	
	public class EngulfingBarsAutoTrader : Strategy
	{
		
		
		private const string SystemVersion 					= " V1.0";
        private const string SystemName 					= "EngulfingBarsAutoTrader";
		
		
		private double priceOffset 							= 0.01;
		private double percentageOffset 					= 0.00;
		private int tickOffset								= 1;
		
		private bool engulfBody;
		private bool currentBodyEngulfGreen;
		private bool currentBodyEngulfRed;
		
		private bool colorOutsideBar;
		
		
		private bool currentOBGreen;
		private bool currentOBRed;
		
		private int count;
		private bool setChartOB;
		

		
	
		
		
		private double mdl;
		
		private double mdp;
		
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"AutoTrader For Engulfing Bars.   Based 
                                                                 off of the Engulfing Indicator from Matt LaRoco";
				Name										= @"EngulfingBarsAutoTrader";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				//IsSuspendedWhileInactive					= true;
				
		
				///Default colors for Outside Bars
				colorOutsideBar								= true;
				GreenOutsideBar 							= Brushes.RoyalBlue;
				RedOutsideBar								= Brushes.DarkMagenta;
				
				
				///Current or Previous Outside Bars condition is true
				currentOBGreen								= false;
				currentOBRed								= false;
				
		
				
				engulfBody									= false;
				currentBodyEngulfGreen						= false;
				currentBodyEngulfRed						= false;
				
				///Auto Trader
				autoTrade                                   =true;
				
				count=0;
				
		
				
				
			}
			else if (State == State.Configure)
			{
			}
			
		else if (State == State.DataLoaded)
			{				
				
			}
			
		}
		
		protected override void OnBarUpdate()
		{
			
			if(CurrentBar < 2) return;
			
			count++;
			
			BarBrush	= null;
			BackBrushes = null;
			
	#region offset Calculations
			
			//Percent for the previous Outside Bar
			double percentCalcAlert = (High[2] - Low[2]) * percentageOffset;
			 
			//Percent for the current OB forming
			double percentageCalc = (High[1] - Low[1]) * percentageOffset;
			
			//These are the same for both because they do not rely on another candle
			double priceCalc = 	priceOffset;
			double tickCalc = TickSize * tickOffset;
			
			//Picks Highest Offset for the Current OB
			double OutsideBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));
			
			//Picks Highest Offset for previous OB (Alert OB's)
			double OutsideBarOffsetAlert = Math.Max(percentCalcAlert, Math.Max(priceCalc, tickCalc));
	
	#endregion		
		
	#region Current Outside Bar Logic
		
		///Current Outside Bars 'Without' Engulf Body Selected	
			
			//Bullish Engulfing
			if (
				(engulfBody == false)
					&& Open[1] > Close[1]
					&& Close[0] > Open[0] 
					&& Close[0] >= Open[1]
					&& Close[1] >= Open[0]
					&& (Close[0] - Open[0]) > (Open[1] - Close[1]))
					
			
			{
				currentOBGreen = true;
			}
				else
			{
				currentOBGreen = false;	
			}
			
			
			//Bearish Engulfing	
			if (
				(engulfBody == false)
					&& Close[1] > Open[1]
					&& Open[0] > Close[0] 
					&& Open[0] >= Close[1]
					&& Open[1] >= Close[0]
					&& (Open[0] - Close[0]) > (Close[1] - Open[1]))
			
			{
				currentOBRed = true;
			}
				else
			{
				currentOBRed = false;	
			}
			
		///Current Outside Bars with Engulf Body Selected	
			
			//Green Outside Bar w/ Engulf Body
			if (
				(engulfBody)
					&&((High[0]-(OutsideBarOffset)) >= High[1]) 
					&& ((Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] < Close[0])
							&& (Open[1] > Close[1])
								&& ((Open[0] <= Close[1]) && Close[0] >= Open[1])
				)
			
			{
				currentBodyEngulfGreen = true;
			}
				else
			{
				currentBodyEngulfGreen = false;
			}
			
			//Red Outside Bar w/ Engulf Body
			if (
				(engulfBody)
					&&((High[0]-(OutsideBarOffset)) >= High[1]) 
					&& ((Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] > Close[0])
							&& (Open[1] < Close[1])
								&& ((Open[0] >= Close[1]) && Close[0] <= Open[1])
				)
			
			{
				currentBodyEngulfRed = true;
			}
				else
			{
				currentBodyEngulfRed = false;
			}
			
			
			
			
		
	#endregion
		
	
	
	if (setChartOB)
		{
			
	#region Color Outside Bars
			
	///Outside Bar Color
		if (colorOutsideBar)
		{
			//Green Outside Bar Logic - Current Bar is Green, engulfing a previous Red candle
			if (currentOBGreen) 
				
				{
					BarBrush = GreenOutsideBar;
				}
			
			//Red Outside Bar Logic - Current Bar is Red, engulfing a previous Green candle
			else if (currentOBRed) 
					
				{
					BarBrush = RedOutsideBar;
				}
			
		///Needs body of current candle to be greater than body of previous candle		
		
				//Green Outside Bar Logic - Current Bar is Green, engulfing a previous Red candle
			if (currentBodyEngulfGreen)
					
				{
					BarBrush = GreenOutsideBar;
				}	
			
				//Red Outside Bar Logic - Current Bar is Red, engulfing a previous Green candle 
			else if (currentBodyEngulfGreen)
				
				{
					BarBrush = RedOutsideBar;
				}
	
		}
	
	#endregion
	
	
	
			
	
			
		}	
		
		
		#region AutoTrader
		
		 //Exit Logic-Uses check on previous bar engulfing status for exit logic
		if(Position.MarketPosition==MarketPosition.Long /*&& count==2*/){
			ExitLong();
			count=0;
		}
		else if(Position.MarketPosition==MarketPosition.Short /*&& count==2*/){
			ExitShort();
			count=0;
		}
		
		  if(autoTrade && Position.MarketPosition==MarketPosition.Flat){
		  	if(currentOBGreen || currentBodyEngulfGreen){
				
				
				EnterLong();
				SetStopLoss("",CalculationMode.Price,Low[0],false);
				count=1;
					
				
				
				
			}
			
			if(currentOBRed  || currentBodyEngulfRed){
				
				EnterShort();
				SetStopLoss("",CalculationMode.Price,High[0],false);
				count=1;
				
			}
			
			
			  
			  
			  
		  }
		
		
		#endregion
	
	
			
			else
			{
				return;
			}
		}
			
		
	
		#region Properties
	
		#region 1. Auto Trader
		/// <summary>
		/// Auto Trader Settings
		/// </summary>
		[Display(Name = "Use AutoTrader?", GroupName = "AutoTrader", Order = 0)]
		public bool autoTrade
		
			{get;set;}
		
			
		
		
		
			
		
			
		#endregion
		
		#region 2. Offset Properties
	///Offset for Outside Bars. Allows Price to come outside of the High/Low of the previous candle if the user chooses.
		
		[Display(Name = "Price Offset", GroupName = "Outside Bar Offset", Order = 0)]
		public double PriceOffset
		{
			get{return priceOffset;}
			set{priceOffset = (value);}
		}
			
		[Display(Name = "Percentage Offset", GroupName = "Outside Bar Offset", Order = 1)]
		public double PercentageOffset
		{
			get{return percentageOffset;}
			set{percentageOffset = (value);}
		}
		
		[Display(Name = "Tick Offset", GroupName = "Outside Bar Offset", Order = 2)]
		public int TickOffset
		{
			get{return tickOffset;}
			set{tickOffset = (value);}
		}
		#endregion
		
		#region 3. Color Outside Bars
	///Change the color of the Green and Red Outside Bars.
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Color Outside Bars", Description = "", Order = 0, GroupName = "Outside Bar Custom Color")]
		public bool ColorOutsideBar 
		{
		 	get{return colorOutsideBar;} 
			set{colorOutsideBar = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Engulf Body of previous candle", Description = "Body of the current candle must be greater than body of the previous candle", Order = 1, GroupName = "Outside Bar Custom Color")]
		public bool EngulfBody 
		{
		 	get{return engulfBody;} 
			set{engulfBody = (value);} 
		}
		
		[XmlIgnore()]
		[Display(Name = "Bullish Outside Bar", GroupName = "Outside Bar Custom Color", Order = 2)]
		public Brush GreenOutsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string GreenOutsideBarSerialize
		{
			get { return Serialize.BrushToString(GreenOutsideBar); }
   			set { GreenOutsideBar = Serialize.StringToBrush(value); }
		}
		
			[XmlIgnore()]
		[Display(Name = "Bearish Outside Bar", GroupName = "Outside Bar Custom Color", Order = 3)]
		public Brush RedOutsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string RedOutsideBarSerialize
		{
			get { return Serialize.BrushToString(RedOutsideBar); }
   			set { RedOutsideBar = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		
		
		
		
		#endregion
		
	}
}






























































