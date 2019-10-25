using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ED_Explorator_Companion
{
    internal partial class SystemDataGrid : Form
    {
        public SystemDataGrid()
        {
            InitializeComponent();
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add(new string[] { "", "PLEASE", "WAIT", "STARTING", "PLEASE", "WAIT", "" });
        }

        internal void UpdateStatus(string state)
        {
            if (statusStrip1 == null || !IsHandleCreated) return;
            statusStrip1.Invoke(
                new MethodInvoker(
                    delegate
                    {
                        toolStripStatusLabel1.Text = state;
                    }
                )
            );
        }

        internal void updateGrid(StarSystem currentSys, List<StarSystem> nearSystems)
        {
            dataGridView1.Invoke(
                new MethodInvoker(
                    delegate
                    {
                        var i = 0;
                        foreach (var sys in nearSystems)
                        {
                            var str = new string[] {
                                sys.SystemName,
                                String.Format("{0:F2}", sys.DistanceFrom(currentSys)),
                                sys.Visites.ToString(),
                                sys.BodiesScanned.ToString(),
                                sys.BodiesCount.ToString(),
                                sys.AllBodiesFound.ToString(),
                                sys.getInterrestingBodies()
                            };
                            var color = Color.White;
                            if (sys.AllBodiesFound)
                                color = Color.Green;
                            else if (sys.Bodies.Count() == 0)
                                color = Color.Gray;
                            else if (sys.BodiesScanned != 0 && !sys.AllBodiesFound)
                                color = Color.Yellow;
                            else if (sys.Bodies.Where(b => b.TerraformState == "Terraformable" || b.BodyType == "Ammonia world" || b.BodyType == "Earthlike body" || b.BodyType == "Water world").Count() != 0)
                                color = Color.LightBlue;
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = color;
                            ++i;
                        }
                        while (i < dataGridView1.Rows.Count)
                            dataGridView1.Rows.RemoveAt(i);
                    }
                )
            );
        }

        internal void UpdateImport(int count, StarSystem sysToImport, double distance)
        {
            if (!IsHandleCreated) return;
            statusStrip1.Invoke(new MethodInvoker(
            delegate
            {
                toolStripStatusLabel2.Text = count + " system To import ( " + sysToImport.SystemName + " being imported (" + Math.Round(distance, 2) + " ly))";
            }));
        }

        internal void UpdateFirstLine(StarSystem currentSys)
        {
            dataGridView1.Invoke(new MethodInvoker(
       delegate
       {
           if (dataGridView1.Rows.Count == 0)
               dataGridView1.Rows.Add();
           dataGridView1.Rows[0].SetValues(new string[] {
                        currentSys.SystemName,
                        "0.00",
                        currentSys.Visites.ToString(),
                        currentSys.BodiesScanned.ToString(),
                        currentSys.BodiesCount.ToString(),
                        currentSys.AllBodiesFound.ToString(),
                        currentSys.getInterrestingBodies()
           });

           var color = Color.White;
           if (currentSys.AllBodiesFound)
               color = Color.Green;
           else if (!currentSys.AllBodiesFound)
               color = Color.Yellow;
           dataGridView1.Rows[0].DefaultCellStyle.BackColor = color;
       }));
        }
    }
}