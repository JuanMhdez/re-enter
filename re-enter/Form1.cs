using re_enter.Runcard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace re_enter
{
    public partial class Form1: Form
    {
        // EWO-5101

        runcard_wsdlPortTypeClient cliente = new runcard_wsdlPortTypeClient("runcard_wsdlPort");


        public Form1()
        {
            InitializeComponent();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                //Get App Version
                Version ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                label3.Text = ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            if(e.KeyCode == Keys.Enter)
            {
                bool estatus;

                if(textBox1.Text != string.Empty)
                {

                    string serial = textBox1.Text.Trim();
                    string msg;
                    int error;



                    var status = cliente.getUnitStatus(serial, out error,out msg);

                    if (error == 0)
                    {


                        if (status.seqnum == 40 && status.opcode == "A217")
                        {

                            if(status.status == "ON HOLD")
                            {


                                // Transaccion

                                transactionItem request = new transactionItem()
                                {

                                    username = "ftest",
                                    transaction = "RELEASE",
                                    workorder = status.workorder,
                                    serial = status.serial,
                                    trans_qty = 1,
                                    seqnum = status.seqnum,
                                    opcode = status.opcode,
                                    warehouseloc = status.warehouseloc,
                                    warehousebin = status.warehousebin,
                                    comment = "[RPC] TRANSACCION ECHA POR SISTEMA"

                                };

                                // dataitem
                                dataItem[] inputData = new dataItem[] { };

                                // Bom Item en este caso no se conume ninguna pieza del bom

                                bomItem[] bomData = new bomItem[] { };


                                var transaccion = cliente.transactUnit(request, inputData, bomData, out msg);


                                if (error == 0)
                                {

                                    mostrarMensaje($"Pieza {status.serial} liberada de HOLD, vuelva a ingresar la pieza",estatus=true);

                                }


                            }
                            else
                            {
                                mostrarMensaje($"La pieza {status.serial} no esta en HOLD. La pieza esta " + status.status,estatus=false);
                                
                            }

                        }
                        else
                        {
                            mostrarMensaje($"La pieza {status.serial} se encuentra en otra estacion, se encuentra en: " + status.opcode,estatus = false);
                        }


                    }
                    else
                    {
                        mostrarMensaje("Error al consultar el estatus del serial", estatus = false);
                      
                    }

                }
                else
                {
                    mostrarMensaje("Campo vacio",estatus = false);
                   
                }


                textBox1.Text = string.Empty;


            }




        }


        public void mostrarMensaje(string mensaje, bool status)
        {

            // Verificar si ya existe una ventana dentro del panel
            foreach (Control control in panel1.Controls)
            {
                if (control is Form)
                {
                    // Si hay una ventana, la eliminamos
                    control.Dispose();
                }
            }
            Mensaje fmensaje = new Mensaje(mensaje);

            if (status)
            {
                fmensaje.BackColor = System.Drawing.Color.FromArgb(0, 127, 22); // Color verde para éxito
            }
            else
            {
                fmensaje.BackColor = System.Drawing.Color.FromArgb(196, 24, 1); // Color rojo para error
            }


            fmensaje.TopLevel = false;
            fmensaje.Parent = panel1;
            fmensaje.Size = panel1.ClientSize;
            fmensaje.Show();
        }


    }
}
