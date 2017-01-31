using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;

using NeuroSky.ThinkGear;
using NeuroSky.ThinkGear.Algorithms;

using NeuronDotNet.Core;
using NeuronDotNet.Core.Backpropagation;

 

namespace MindWave
{
    public partial class Form1 : Form
    {
       static Connector connector;
       int  nchar = 0;
       public static BackpropagationNetwork network;
       bool eegTime = false;
       public volatile bool _shouldStop = false;
       public volatile bool _shouldStop2 = false;
       bool closeAllThread = false;
       bool atflag = false;
       
             public Form1()
        {
       
            InitializeComponent();
       
          

        }

     
       
        public void connect()
        {

          
            // Initialize a new Connector and add event handlers
            connector = new Connector();
            connector.DeviceConnected += new EventHandler(OnDeviceConnected);
            connector.DeviceConnectFail += new EventHandler(OnDeviceFail);
            connector.DeviceValidating += new EventHandler(OnDeviceValidating);
         

           // Scan for devices
            connector.ConnectScan("COM12");

            //Thread.Sleep(450000);

        }

       
      
        // Called when a device is connected 
       public void OnDeviceConnected(object sender, EventArgs e)
        {
            Connector.DeviceEventArgs de = (Connector.DeviceEventArgs)e;

            label14.Invoke((MethodInvoker)(() =>  label14.Text = "Device found on: " + de.Device.PortName));
           // MessageBox.Show( "Device found on: "  + de.Device.PortName);
            de.Device.DataReceived += new EventHandler(OnDataReceived);
          
         

        }

        /**
         * Called when scanning fails
         */
        public void OnDeviceFail(object sender, EventArgs e)
        {
             label14.Invoke((MethodInvoker)(() => label14.Text = "No devices found! :("));
           // MessageBox.Show( "No devices found! :(");
        }

        /**
         * Called when each port is being validated
         */
        public void OnDeviceValidating(object sender, EventArgs e)
        {
          // MessageBox.Show ("Validating...");
            label14.Invoke((MethodInvoker)(() => label14.Text = "Validating..."));
         
        }

        /**
         * Called when data is received from a device
         */
   public  void OnDataReceived(object sender, EventArgs e)
        {
             Device.DataEventArgs de = (Device.DataEventArgs)e;


             if (!closeAllThread)
             {

                 var tRaw = new Thread(() => plotRaw(de));
                 tRaw.Start();

                 var tParam = new Thread(() => plotParam(de));
                 tParam.Start();

                 var tWaves = new Thread(() => plotWaves(de));
                 tWaves.Start();
             }
        
   }

   private void plotParam(Device.DataEventArgs de)
   {

       NeuroSky.ThinkGear.DataRow[] tempDataRowArray = de.DataRowArray;

       TGParser tgParser = new TGParser();
       tgParser.Read(de.DataRowArray);

       for (int i = 0; i < tgParser.ParsedData.Length; i++)
       {

           if (tgParser.ParsedData[i].ContainsKey("PoorSignal"))
           {
               label2.Invoke((MethodInvoker)(() => label2.Text = "Signal: " + (100 - tgParser.ParsedData[i]["PoorSignal"]) + "%"));
           }

           if (tgParser.ParsedData[i].ContainsKey("Attention"))
           {
               label3.Invoke((MethodInvoker)(() => label3.Text = "Attention: " + tgParser.ParsedData[i]["Attention"]));
               progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = (int)tgParser.ParsedData[i]["Attention"]));
              // if (tgParser.ParsedData[i]["Attention"] > 90 && atflag == false)
               //{
                 //     System.Diagnostics.Process.Start("C:\\Users\\Rs\\Documents\\test.mp3");
                   //   atflag = true;
              // }


           }

           if (tgParser.ParsedData[i].ContainsKey("Meditation"))
           {
               label4.Invoke((MethodInvoker)(() => label4.Text = "Meditation: " + tgParser.ParsedData[i]["Meditation"]));
               progressBar2.Invoke((MethodInvoker)(() => progressBar2.Value = (int)tgParser.ParsedData[i]["Meditation"]));
               if (tgParser.ParsedData[i]["Meditation"] > 90 && atflag == false)
               {
                   System.Diagnostics.Process.Start("C:\\Users\\Rs\\Documents\\test.mp3");
                   //atflag = true;
               }
           }



           if (tgParser.ParsedData[i].ContainsKey("BlinkStrength"))
           {
               label13.Invoke((MethodInvoker)(() => label13.Text = "Blink: " + tgParser.ParsedData[i]["BlinkStrength"]));
           }


           if (closeAllThread)
           {
               Thread.CurrentThread.Abort();
           }

       }
   }

   private void plotWaves(Device.DataEventArgs de)
   {

       NeuroSky.ThinkGear.DataRow[] tempDataRowArray = de.DataRowArray;

       TGParser tgParser = new TGParser();
       tgParser.Read(de.DataRowArray);

       for (int i = 0; i < tgParser.ParsedData.Length; i++)
       {

           if (tgParser.ParsedData[i].ContainsKey("EegPowerDelta") && tgParser.ParsedData[i]["EegPowerDelta"] > 0)
           {
               label5.Invoke((MethodInvoker)(() => label5.Text = "Delta: " + tgParser.ParsedData[i]["EegPowerDelta"]));
               chart1.Invoke((MethodInvoker)(() => chart1.Series["Delta"].Points.AddY(tgParser.ParsedData[i]["EegPowerDelta"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["Delta"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerDelta"]))));
        
               if (chart1.Series["Delta"].Points.Count > 20)
               {
                   chart1.Invoke((MethodInvoker)(() => chart1.Series["Delta"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["Delta"].Points.RemoveAt(0)));
                   button6.Invoke((MethodInvoker)(() => button6.Enabled = true));
               }

               chart7.Invoke((MethodInvoker)(() => chart7.ChartAreas[0].AxisY.Maximum= 15));
               chart7.Invoke((MethodInvoker)(() => chart7.ChartAreas[0].AxisY.Minimum = 5));
               chart2.Invoke((MethodInvoker)(() => chart2.ChartAreas[0].AxisY.Maximum = 1000000));

           }

           if (tgParser.ParsedData[i].ContainsKey("EegPowerTheta") && tgParser.ParsedData[i]["EegPowerTheta"] > 0)
           {
               label6.Invoke((MethodInvoker)(() => label6.Text = "Theta: " + tgParser.ParsedData[i]["EegPowerTheta"]));
               chart2.Invoke((MethodInvoker)(() => chart2.Series["Theta"].Points.AddY(tgParser.ParsedData[i]["EegPowerTheta"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["Theta"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerTheta"]))));
           
               if (chart2.Series["Theta"].Points.Count > 20)
               {
                   chart2.Invoke((MethodInvoker)(() => chart2.Series["Theta"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["Theta"].Points.RemoveAt(0)));
               }
               
               chart2.Invoke((MethodInvoker)(() => chart2.ChartAreas[0].AxisY.Maximum = 400000));

           }

           if (tgParser.ParsedData[i].ContainsKey("EegPowerAlpha1") && tgParser.ParsedData[i]["EegPowerAlpha1"] > 0)
           {
               label7.Invoke((MethodInvoker)(() => label7.Text = "Low Alpha: " + tgParser.ParsedData[i]["EegPowerAlpha1"]));
               chart3.Invoke((MethodInvoker)(() => chart3.Series["Low Alpha"].Points.AddY(tgParser.ParsedData[i]["EegPowerAlpha1"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Alpha"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerAlpha1"]))));
            
               if (chart3.Series["Low Alpha"].Points.Count > 20)
               {
                   chart3.Invoke((MethodInvoker)(() => chart3.Series["Low Alpha"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Alpha"].Points.RemoveAt(0)));
               }

                chart3.Invoke((MethodInvoker)(() => chart3.ChartAreas[0].AxisY.Maximum = 100000));

           }

           if (tgParser.ParsedData[i].ContainsKey("EegPowerAlpha2") && tgParser.ParsedData[i]["EegPowerAlpha2"] > 0)
           {
               label8.Invoke((MethodInvoker)(() => label8.Text = "High Alpha: " + tgParser.ParsedData[i]["EegPowerAlpha2"]));
               chart3.Invoke((MethodInvoker)(() => chart3.Series["High Alpha"].Points.AddY(tgParser.ParsedData[i]["EegPowerAlpha2"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["High Alpha"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerAlpha2"]))));
               
               if (chart3.Series["High Alpha"].Points.Count > 20)
               {
                   chart3.Invoke((MethodInvoker)(() => chart3.Series["High Alpha"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["High Alpha"].Points.RemoveAt(0)));
               }


           }

           if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta1") && tgParser.ParsedData[i]["EegPowerAlpha1"] > 0)
           {
               label9.Invoke((MethodInvoker)(() => label9.Text = "Low Beta: " + tgParser.ParsedData[i]["EegPowerBeta1"]));
               chart4.Invoke((MethodInvoker)(() => chart4.Series["Low Beta"].Points.AddY(tgParser.ParsedData[i]["EegPowerBeta1"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Beta"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerBeta1"]))));
             
               if (chart4.Series["Low Beta"].Points.Count > 20)
               {
                   chart4.Invoke((MethodInvoker)(() => chart4.Series["Low Beta"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Beta"].Points.RemoveAt(0)));
               }

                chart4.Invoke((MethodInvoker)(() => chart4.ChartAreas[0].AxisY.Maximum = 50000));
               //  chart4.Invoke((MethodInvoker)(() => chart4.ChartAreas[0].AxisY.Minimum = 5));

           }

           if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta2") && tgParser.ParsedData[i]["EegPowerBeta2"] > 0)
           {
               label10.Invoke((MethodInvoker)(() => label10.Text = "High Beta: " + tgParser.ParsedData[i]["EegPowerBeta2"]));
               chart4.Invoke((MethodInvoker)(() => chart4.Series["High Beta"].Points.AddY(tgParser.ParsedData[i]["EegPowerBeta2"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["High Beta"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerBeta2"]))));
            
               
               if (chart4.Series["High Beta"].Points.Count > 20)
               {
                   chart4.Invoke((MethodInvoker)(() => chart4.Series["High Beta"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["High Beta"].Points.RemoveAt(0)));
               }

           }


           if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma1") && tgParser.ParsedData[i]["EegPowerGamma1"] > 0)
           {
               label11.Invoke((MethodInvoker)(() => label11.Text = "Low Gamma: " + tgParser.ParsedData[i]["EegPowerGamma1"]));
               chart5.Invoke((MethodInvoker)(() => chart5.Series["Low Gamma"].Points.AddY(tgParser.ParsedData[i]["EegPowerGamma1"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Gamma"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerGamma1"]))));
              
               if (chart5.Series["Low Gamma"].Points.Count > 20)
               {
                   chart5.Invoke((MethodInvoker)(() => chart5.Series["Low Gamma"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["Low Gamma"].Points.RemoveAt(0)));
               }

                chart5.Invoke((MethodInvoker)(() => chart5.ChartAreas[0].AxisY.Maximum = 20000));

           }

         
           if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma2") && tgParser.ParsedData[i]["EegPowerGamma2"] > 0)
           {
               label12.Invoke((MethodInvoker)(() => label12.Text = "High Gamma: " + tgParser.ParsedData[i]["EegPowerGamma2"]));
               chart5.Invoke((MethodInvoker)(() => chart5.Series["High Gamma"].Points.AddY(tgParser.ParsedData[i]["EegPowerGamma2"])));
               chart7.Invoke((MethodInvoker)(() => chart7.Series["High Gamma"].Points.AddY(Math.Log(tgParser.ParsedData[i]["EegPowerGamma2"]))));
             
               if (chart5.Series["High Gamma"].Points.Count > 20)
               {
                   chart5.Invoke((MethodInvoker)(() => chart5.Series["High Gamma"].Points.RemoveAt(0)));
                   chart7.Invoke((MethodInvoker)(() => chart7.Series["High Gamma"].Points.RemoveAt(0)));
               }


           }

           if (closeAllThread)
           {
               Thread.CurrentThread.Abort();
           }

       }
   }

   private void plotRaw(Device.DataEventArgs de)
   {

       NeuroSky.ThinkGear.DataRow[] tempDataRowArray = de.DataRowArray;

       TGParser tgParser = new TGParser();
       tgParser.Read(de.DataRowArray);

       if (eegTime && closeAllThread == false)
       {

           chart6.Invoke((MethodInvoker)(() => chart6.ChartAreas[0].AxisY.Minimum = -400));
           chart6.Invoke((MethodInvoker)(() => chart6.ChartAreas[0].AxisY.Maximum = 400));
           for (int i = 0; i < tgParser.ParsedData.Length; i++)
           {
               if (tgParser.ParsedData[i].ContainsKey("Raw"))
               {
                   eegTime = false;
                   chart6.Invoke((MethodInvoker)(() => chart6.Series["EEG"].Points.AddY(tgParser.ParsedData[i]["Raw"])));
               }
               if (chart6.Series["EEG"].Points.Count > 200)
               {
                   chart6.Invoke((MethodInvoker)(() => chart6.Series["EEG"].Points.RemoveAt(0)));
               }
           }
       }
       
       Thread.CurrentThread.Abort();
   }
   
   private void button2_Click(object sender, EventArgs e)
   {
       closeAllThread = true;
       connector.Close();
          // Environment.Exit(0);
    
   }

   private void button3_Click(object sender, EventArgs e)
   {
       connect();
       timer2.Enabled = true;
   }

   private void chart2_Click(object sender, EventArgs e)
   {

   }

 public void button4_Click(object sender, EventArgs e)
   {
       var openWin = new OpenFileDialog();
       openWin.DefaultExt = "txt";
       openWin.ShowDialog();
       string path = openWin.FileName;

       int nInput = Convert.ToInt32(textBox3.Text);
       int nOut = Convert.ToInt32(textBox5.Text);

       TrainingSet train = new TrainingSet(nInput, nOut);
       string[] lines = System.IO.File.ReadAllLines(path);
       string[] trainData = new string[nInput + nOut];
       double[] trainInput = new double[nInput];
      double[] trainOut = new double[nOut];

       foreach (string line in lines)
       {
           trainData = line.Split(' ');

           for (int i = 0; i < nInput; i++)
           {
               trainInput[i] = Convert.ToDouble(trainData[i]);
           }

           for (int i = nInput; i < nOut; i++)
           {
               trainOut[i - nInput] = Convert.ToDouble(trainData[i]);
           }

           
          train.Add(new TrainingSample(trainInput, trainOut));
          


       }

       network.Learn(train, Convert.ToInt32(textBox6.Text));
       MessageBox.Show("Training OK");

   }

 private void timer1_Tick(object sender, EventArgs e)
 {

    
      string[] lettere = new string[21] {"A","B","C","D","E","F","G","H","I","L","M","N","O","P","Q","R","S","T","U","V","Z"};
      
    
      if (nchar > 19)
      {
          nchar = 0;
          timer1.Enabled = false;
          MessageBox.Show("Train finished");
          _shouldStop2 = true;

      }
      else { nchar += 1; }

    //  label16.Text = lettere[nchar];

 }

 private void button5_Click(object sender, EventArgs e)
 {
     var t3 = new Thread(() => testing());
     t3.Start();
     
  
 }

 private void label5_Click(object sender, EventArgs e)
 {

 }

 public void testing()
 {
     double[] dati = new double[10];

     while (_shouldStop2 == false)
     {
         if (dati[0] != chart1.Series["Delta"].Points[19].YValues[0])
         {
             dati[0] = chart1.Series["Delta"].Points[19].YValues[0];
             dati[1] = chart2.Series["Theta"].Points[19].YValues[0];
             dati[2] = chart3.Series["Low Alpha"].Points[19].YValues[0];
             dati[3] = chart3.Series["High Alpha"].Points[19].YValues[0];
             dati[4] = chart4.Series["Low Beta"].Points[19].YValues[0];
             dati[5] = chart4.Series["High Beta"].Points[19].YValues[0];
             dati[6] = chart5.Series["Low Gamma"].Points[19].YValues[0];
             dati[7] = chart5.Series["High Gamma"].Points[19].YValues[0];
             dati[8] = progressBar1.Value;
             dati[9] = progressBar2.Value;

            double[] output = network.Run(dati);
           // label17.Invoke((MethodInvoker)(() => label17.Text = "" + output[0]));

         }
     }
     

 }

 public void saveData(string path)
 {
     
     double[] dati = new double[10];
   

    while (_shouldStop == false){
         if (dati[0] != chart1.Series["Delta"].Points[19].YValues[0]) 
         { 
             dati[0] = chart1.Series["Delta"].Points[19].YValues[0];
             dati[1] = chart2.Series["Theta"].Points[19].YValues[0];
             dati[2] = chart3.Series["Low Alpha"].Points[19].YValues[0];
             dati[3] = chart3.Series["High Alpha"].Points[19].YValues[0];
             dati[4] = chart4.Series["Low Beta"].Points[19].YValues[0];
             dati[5] = chart4.Series["High Beta"].Points[19].YValues[0];
             dati[6] = chart5.Series["Low Gamma"].Points[19].YValues[0];
             dati[7] = chart5.Series["High Gamma"].Points[19].YValues[0];
             dati[8] = progressBar1.Value;
             dati[9] = progressBar2.Value;



             using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
             {

                 string lineData = "";
                 for (int i = 0; i < 10; i++)
                 {
                     lineData += dati[i] + " ";
                 }
      
                 file.WriteLine(lineData);
             }

        }
    } Thread.CurrentThread.Abort();
 }

public void button6_Click(object sender, EventArgs e)
 {
     var saveWin = new SaveFileDialog();
     saveWin.DefaultExt = "txt";
     saveWin.ShowDialog();
     string path = saveWin.FileName;

    
    var t2 = new Thread(() => saveData(path));
     t2.Start();
     label18.Text = "Recording...";
   
 }

 private void button7_Click(object sender, EventArgs e)
 {
     _shouldStop = true;
     label18.Text = "Rec: OFF";

 }


 private void timer2_Tick(object sender, EventArgs e)
 {
     eegTime = true;
 }

 private void label18_Click(object sender, EventArgs e)
 {

 }

 private void button8_Click(object sender, EventArgs e)
 {

    LinearLayer inputLayer = new LinearLayer(Convert.ToInt32(textBox3.Text));
    SigmoidLayer hiddenLayer = new SigmoidLayer(Convert.ToInt32(textBox4.Text));
    SigmoidLayer outputLayer = new SigmoidLayer(Convert.ToInt32(textBox5.Text));
   
   
     BackpropagationConnector conn1 = new BackpropagationConnector(inputLayer, hiddenLayer);
     BackpropagationConnector conn2 = new BackpropagationConnector(hiddenLayer, outputLayer);

     network = new BackpropagationNetwork(inputLayer, outputLayer);
     network.Initialize();

     MessageBox.Show("Rete generata con successo.");
 }

 private void textBox6_TextChanged(object sender, EventArgs e)
 {

 }

 private void pictureBox2_Click(object sender, EventArgs e)
 {

 }

 private void button9_Click(object sender, EventArgs e)
 {
     _shouldStop2 = true;
 }

 private void label16_Click(object sender, EventArgs e)
 {

 }


   
    
  

    
 





    }
}
