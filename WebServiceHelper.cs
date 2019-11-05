using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Web.Services.Description;

namespace UploadElectronicMedicalRecord.Utils
{
    public class WebServiceHelper
    {
        /// <summary>
        /// WebService 超时设置
        /// </summary>
        private int outTime = 1000;
        /// <summary>
        /// WebService 超时设置
        /// </summary>
        public int OutTime
        {
            get { return this.outTime; }
            set { this.outTime = value; }
        }

        #region InvokeWebService
        /// < summary>
        /// 动态调用web服务
        /// < /summary>
        /// < param name="url">WSDL服务地址< /param>
        /// < param name="classname">类名< /param>
        /// < param name="methodname">方法名< /param> 
        /// < param name="args">参数< /param> 
        /// < returns>< /returns> 
        public object InvokeWebService(string url, string classname, string methodname, object arg)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            try
            {
                //获取WSDL 
                WebClient wc = new WebClient();
                if (!url.ToUpper().Contains("WSDL"))
                {
                    url = string.Format("{0}?{1}", url, "WSDL");
                }
                Stream stream = wc.OpenRead(url);
                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);
                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();
                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");
                //编译代理类 
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                //生成代理实例，并调用方法  
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                //设置WebService超时时间
                ((System.Web.Services.Protocols.WebClientProtocol)(obj)).Timeout = this.outTime;
                var jsonStr = JsonConvert.SerializeObject(arg);
                var miParType = mi.GetParameters()[0].ParameterType;
                return mi.Invoke(obj, new object[] { JsonConvert.DeserializeObject(jsonStr, miParType) });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
