using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ED_Explorator_Companion
{
    internal partial class SystemDataGrid : Form
    {
        public SystemDataGrid()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Rows.Clear();

            dataGridView2.Rows.Add(new string[] { "Near blue system", "?", "? ly" });
            dataGridView2.Rows.Add(new string[] { "Near yellow system", "?", "? ly" });
            dataGridView2.Rows.Add(new string[] { "Near gray system", "?", "? ly" });
        }

        internal void UpdateStatus(string state)
        {
            while (!IsHandleCreated) Thread.Sleep(100);
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
            while (!IsHandleCreated) Thread.Sleep(100);
            dataGridView1.Invoke(
                new MethodInvoker(
                    delegate
                    {
                        dataGridView1.DataSource = nearSystems;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            var sys = (StarSystem)row.DataBoundItem;
                            var color = Color.White;
                            if (sys.AllBodiesFound)
                                color = Color.Green;
                            else if (sys.Bodies.Count == 0)
                                color = Color.Gray;
                            else if (sys.Bodies.Where(b => b.TerraformState == "Terraformable" || b.BodyType == "Ammonia world" || b.BodyType == "Earthlike body" || b.BodyType == "Water world").Count() != 0)
                                color = Color.LightBlue;
                            else if (!sys.AllBodiesFound)
                                color = Color.Yellow;
                            row.DefaultCellStyle.BackColor = color;
                        }
                    }
                )
            );
            dataGridView2.Invoke(
                new MethodInvoker(
                    delegate
                    {
                        Database.mutex.WaitOne();
                        var blueSys = Database.db.GetCollection<StarSystem>().Find(s => !s.AllBodiesFound && (s.Bodies[0].TerraformState == "Terraformable" || s.Bodies[0].BodyType == "Ammonia world" || s.Bodies[0].BodyType == "Earthlike body" || s.Bodies[0].BodyType == "Water world")).OrderBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z)).FirstOrDefault();
                        var yellowSys = Database.db.GetCollection<StarSystem>().Find(s => !s.AllBodiesFound).OrderBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z)).FirstOrDefault();
                        var greySys = Database.db.GetCollection<StarSystem>().Find(s => s.Bodies.Count == 0).OrderBy(s => (s.X - currentSys.X) * (s.X - currentSys.X) + (s.Y - currentSys.Y) * (s.Y - currentSys.Y) + (s.Z - currentSys.Z) * (s.Z - currentSys.Z)).FirstOrDefault();
                        Database.mutex.ReleaseMutex();

                        dataGridView2.Rows.Clear();
                        if (blueSys == null)
                            dataGridView2.Rows.Add(new string[] { "Near blue system", "?", "? ly" });
                        else
                            dataGridView2.Rows.Add(new string[] { "Near blue system", blueSys.SystemName, blueSys.DistanceStr, blueSys.InterrestingBodies });
                        if (yellowSys == null)
                            dataGridView2.Rows.Add(new string[] { "Near yellow system", "?", "? ly" });
                        else
                            dataGridView2.Rows.Add(new string[] { "Near yellow system", yellowSys.SystemName, yellowSys.DistanceStr, yellowSys.InterrestingBodies });
                        if (greySys == null)
                            dataGridView2.Rows.Add(new string[] { "Near gray system", "?", "? ly" });
                        else
                            dataGridView2.Rows.Add(new string[] { "Near gray system", greySys.SystemName, greySys.DistanceStr });
                    }

                )
            );
        }

        internal void UpdateImport(int count, StarSystem sysToImport)
        {
            while (!IsHandleCreated) Thread.Sleep(100);
            statusStrip1.Invoke(new MethodInvoker(
            delegate
            {
                toolStripStatusLabel2.Text = count + " system to import ( " + sysToImport.SystemName + " being imported (" + sysToImport.DistanceStr + "))";
            }));
        }

        internal void UpdateFirstLine()
        {
            while (!IsHandleCreated) Thread.Sleep(100);
            var currentSys = Database.CurrentStarSystem;
            dataGridView1.Invoke(new MethodInvoker(
                delegate
                {
                    if (dataGridView1.Rows.Count == 0)
                        dataGridView1.DataSource = new List<StarSystem> { currentSys };
                    else
                        ((List<StarSystem>)dataGridView1.DataSource)[0] = currentSys;

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