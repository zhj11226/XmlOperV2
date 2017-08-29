using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace com.iwefix.tools
{
    /// 原创： 张静
    /// 由于 c# 中不能声明全局变量 ，所以想实现 jQuery 中的 $('xxx') 只能用下面的折中办法：
    /// 如： 
    /// class a {
    ///  	//复制下面这一行到引用类中，然后就可以直接使用了。
    /// 	XmlOperV2.LoadHandle x = ((o) => { return XmlOperV2.load(o); });	
    /// 	如果提示 lamada 表达式无法使用，可使用下边这边替换，一样的效果
    /// 	XmlOperV2.LoadHandle x = (delegate(object o) { return XmlOperV2.load(o); });
    ///  	public int method() {
    /// 			Console.WriteLine( x("<root></root>").Append(x("<item>abc</item>")).Xml() );
    /// 		}
    /// }
    public class XmlOperV2
    {
        private delegate bool EachHandle2(XmlNode obj);

        public delegate bool EachHandle(XmlOperV2 oper);

        /// <summary>
        /// XmlOperV2.LoadHandle x = (delegate(object o) { return XmlOperV2.load(o); });
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public delegate XmlOperV2 LoadHandle(Object obj);

        #region 全局变量

        private string oldXml = "";

        /// <summary>
        /// 最初的 xml
        /// </summary>
        /// <value>The old xml.</value>
        public string OldXml
        {
            get
            {
                return oldXml;
            }
        }

        private int index = 0;

        /// <summary>
        /// Find 操作后的位置
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        private int position = -1;

        /// <summary>
        /// 在其父节点中的位置
        /// </summary>
        public int Position
        {
            get
            {
                if (position >= 0)
                {
                    return position;
                }
                else
                {
                    for (int c = 0; c < _first(initObj).ParentNode.ChildNodes.Count; c++)
                    {
                        if (_first(initObj).ParentNode.ChildNodes[c] == _first(initObj))
                        {
                            position = c;
                        }
                    }
                    return position;
                }
            }
        }

        /// <summary>
        /// 最终的 xml
        /// </summary>
        /// <value>The final xml.</value>
        public string OuterXml
        {
            get
            {
                switch (initObj.GetType().Name)
                {
                    case "XmlDocument":
                        return DocumentXml;
                    default:
                        XmlNode n = _first(initObj);
                        if (n != null)
                            return n.OuterXml.Replace("xmlns=\"\"", "");
                        else
                            return "";
                }
            }
        }

        public string DocumentXml
        {
            get
            {
                if (xmlDoc != null)
                    return xmlDoc.OuterXml.Replace("xmlns=\"\"", "");
                else
                    return "";
            }
        }

        //用来保存所有的命名空间，主要是为了处理前缀重复的问题
        private List<KeyValuePair<string, string>> lstXmlNs = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> LstXmlNs {
            get {
                return lstXmlNs;
            }
        }

        private XmlNamespaceManager nsmgr = null;
        public XmlNamespaceManager Nsmgr
        {
            get
            {
                if (nsmgr == null)
                {
                    nsmgr = _getNameTable(this.xmlDoc, ref lstXmlNs);
                    return nsmgr;
                }
                else
                {
                    return nsmgr;
                }
            }
        }

        public int Length
        {
            get
            {
                int ret = 0;
                switch (initObj.GetType().Name)
                {
                    case "XmlNode":
                    case "XmlElement":
                    case "XmlDocument":
                        ret = 1;

                        break;
                    case "XmlNodeList":
                    case "XPathNodeList":
                        ret = ((XmlNodeList)initObj).Count;

                        break;
                    default:
                        if (initObj.GetType() == typeof(List<XmlNode>))
                            ret = ((List<XmlNode>)initObj).Count;
                        else
                            ret = 0;
                        break;
                }

                return ret;
            }
        }

        private XmlDocument xmlDoc = new XmlDocument();
        private object initObj = null;



        #endregion

        #region 构造函数

        private XmlOperV2()
        {
        }

        private XmlOperV2(object obj)
        {
            if (obj.GetType() == typeof(XmlOperV2))
                obj = ((XmlOperV2)obj).initObj;

            switch (obj.GetType().Name)
            {
                case "String":
                    if (string.IsNullOrEmpty(obj.ToString())) throw new Exception("XmlOperV2不能实例化空字符串");
                    try
                    {
                        xmlDoc.LoadXml(obj.ToString());

                        this.oldXml = xmlDoc.OuterXml;
                        this.initObj = xmlDoc;
                    }
                    catch (XmlException ex)
                    {
                        //var errorCode = ex.GetBaseException().GetType().GetProperty("HResult", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(ex.GetBaseException(),null);
                        //上头这个可以取到 code 但是好像一个 errorCode 对应了好多个 ex.message , 所以还是用 ex.Message 了。
                        if (ex.Message.IndexOf("有多个根元素") >= 0
                        || ex.Message.IndexOf("There are multiple root elements") >= 0)  
                        {
                            XmlOperV2 tmp = XmlOperV2.load("root").Xml(obj.ToString());
                            this.oldXml = tmp.Xml();

                            List<XmlNode> lstNodes = new List<XmlNode>();
                            for (var c = 0; c < tmp.Childrens().Length; c++) {
                                lstNodes.Add(tmp.Childrens()[c].CloneNode(true));
                            }

                            this.initObj = lstNodes;
                            tmp = null;
                        }
                        else
                        {
                            if (File.Exists(obj.ToString()))
                            {
                                xmlDoc.Load(obj.ToString());
                            }
                            else if (obj.ToString().IndexOf("<") < 0)
                            {
                                if (obj.ToString().IndexOf("/") < 0)
                                {
                                    xmlDoc.LoadXml("<" + (new Regex(@"(\w+)")).Match(obj.ToString()).Groups[1].Value + " />");
                                }
                                else
                                {
                                    xmlDoc.LoadXml(_makeXmlTree(obj.ToString()));
                                }
                            }
                            else
                            {
                                throw new Exception("XmlOperV2 无法构造对象！" + obj.ToString());
                            }

                            this.oldXml = xmlDoc.OuterXml;
                            this.initObj = xmlDoc;
                        }
                    }


                    break;
                case "XmlNode":
                case "XmlElement":
                case "XmlDocument":
                    this.oldXml = ((XmlNode)obj).OuterXml;
                    this.initObj = obj;
                    this.xmlDoc = ((XmlNode)obj).OwnerDocument;
                    break;
                case "XmlNodeList":
                case "XPathNodeList":
                    this.initObj = obj;
                    _each(initObj, (o) =>
                    {
                        this.xmlDoc = o.OwnerDocument;
                        return false;
                    });
                    break;
                default:
                    if (obj.GetType().Name.IndexOf("List") >= 0)
                    {
                        this.initObj = obj;
                        _each(initObj, (o) =>
                        {
                            this.xmlDoc = o.OwnerDocument;
                            return false;
                        });
                    }
                    else
                    {
                        throw new Exception("XmlOperV2::XmlOperV2 不接受的构造类型！");
                    }
                    break;
            }
        }

        /// <summary>
        /// 对象类似于 jQuery 的 $($("")[0]) 的作用
        /// 还可以为字符串，
        /// 如果是 "abc" 则会构造成 <abc></abc>
        /// 如果是 "root/item" 则会构造成 <root><item></item></root>
        /// 如果是 "root/item[a=1, innerText=kk]" 则会构造成 <root><item a="1">kk</item></root>
        /// 也可以直接传 xml 字符串 "<root><item></item></root>"
        /// 更复杂的如："root/item[Name=张静]/(age[innerText=34]|sex[innerText=男])"
        ///     会生成：'<root><item Name="张静"><age>34</age><sex>男</sex></item></root>'
        /// </summary>
        /// <param name="o">O.</param>
        public static XmlOperV2 load(object obj)
        {
            XmlOperV2 op = new XmlOperV2(obj);
            op.nsmgr = _getNameTable(op.xmlDoc, ref op.lstXmlNs);
            return op;
        }

        private static XmlOperV2 load(object obj, XmlNamespaceManager nsmgr, int idx)
        {
            XmlOperV2 o = new XmlOperV2(obj);
            o.index = idx;
            o.nsmgr = nsmgr;
            return o;
        }

        #endregion

        #region 公共函数

        public void Each(EachHandle eachCallBack)
        {
            int _index = 0;
            XmlOperV2 me = this;
            _each(initObj, (o) =>
            {
                return eachCallBack(XmlOperV2.load(o, me.nsmgr, _index++));
            });
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public XmlNode this[int index]
        {
            get
            {
                return _eq(initObj, index);
            }
        }

        /// <summary>
        /// 根据 xpath 查找节点，如果xpath里含有命名空间前缀，且该前缀对应多个命名空间，会一个一个去试。
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XmlOperV2 Find(string xpath)
        {
            XmlOperV2 ret = Find(xpath, false);
            if (ret.Length > 0) return ret;

            string[] prefixs = _getXpathPrefix(xpath);
            if (prefixs.Length > 0)
            {
                for (int c = 0; c < prefixs.Length; c++) {
                    var uris = lstXmlNs.FindAll(o => o.Key == prefixs[c]);
                    for (int c2 = 0; c2 < uris.Count; c2++) {
                        var uri = uris[c2];
                        nsmgr.AddNamespace(uri.Key, uri.Value);
                        ret = Find(xpath, false);
                        if (ret.Length > 0) return ret;
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// nsmgrs 格式，直接复制 xml 中的内容，如：xmlns:m="http://www.chinatelecom.com/IMS/MMTelAS/"
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="nsmgrs"></param>
        /// <returns></returns>
        public XmlOperV2 Find(string xpath, params string[] nsmgrs)
        {
            object ret = _find(initObj, xpath, false, nsmgr, false, nsmgrs);
            XmlOperV2 oRet = XmlOperV2.load(ret, nsmgr, 0);
            return oRet;
        }

        /// <summary>
        /// fuzzySearch = true， 支持模糊查询， xpath 前加 //，区分大小写
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="fuzzySearch"></param>
        /// <returns></returns>
        public XmlOperV2 Find(string xpath, bool fuzzySearch)
        {
            object ret = _find(initObj, xpath, fuzzySearch, nsmgr);
            XmlOperV2 oRet = XmlOperV2.load(ret, nsmgr, 0);
            return oRet;
        }

        /// <summary>
        /// fuzzySearch = true， 支持模糊查询， xpath 前加 //，不区分大小写 性能较差
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="fuzzySearch"></param>
        /// <returns></returns>
        public XmlOperV2 FindIgnoreCase(string xpath)
        {
            return XmlOperV2.load(_find(initObj, xpath, true, null, true), nsmgr, 0);
        }

        public XmlOperV2 First()
        {
            return Eq(0);
        }

        public XmlOperV2 Last()
        {
            return Eq(Length - 1);
        }

        public XmlOperV2 Skip(int n)
        {
            return Between(n, Length - 1);
        }

        public XmlOperV2 Eq(int index)
        {
            return XmlOperV2.load(_eq(initObj, index), nsmgr, 0);
        }

        public XmlOperV2 Childrens()
        {
            return Find("./*");
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        public string Text()
        {
            return _text(initObj);
        }

        public string Xml()
        {
            return _xml(initObj);
        }

        public XmlOperV2 Text(string val)
        {
            _text(initObj, val);
            return this;
        }

        public XmlOperV2 Xml(string xml)
        {
            _xml(initObj, xml);
            return this;
        }

        public XmlOperV2 CDATA(string xml)
        {
            this.Xml("");
            XmlCDataSection cdata = this.xmlDoc.CreateCDataSection(xml.ToString());
            this[0].AppendChild(cdata);
            //_xml(initObj, xml);
            return this;
        } 

        public XmlOperV2 Remove()
        {
            XmlNode pNode = null;
            _each(initObj, (o) =>
            {
                pNode = o.ParentNode;
                o.ParentNode.RemoveChild(o);
                return true;
            });
            this.initObj = new List<XmlNode>();
            return this;
        }

        /// <summary>
        /// 添加一个子节点，当前节点不变，如果需要访问刚才添加的节点，需要 Find
        /// </summary>
        /// <param name="node">Node.</param>
        public XmlOperV2 Append(XmlOperV2 node)
        {
            if (initObj == node.initObj)
                throw new Exception("不能把自己附加给自己");

            if (node.xmlDoc != xmlDoc)
            {
                node.xmlDoc = xmlDoc;
                List<XmlNode> lxn = new List<XmlNode>();
                _each(node.initObj, (o) =>
                {
                    lxn.Add(xmlDoc.ImportNode(o, true));
                    return true;
                });
                node.initObj = lxn;
            }

            _each(node.initObj, (o) =>
            {
                _append(initObj, o);
                return true;
            });

            return this;
        }

        public XmlOperV2 Clone()
        {
            if (this.Length == 1)
            {
                return XmlOperV2.load(this[0].CloneNode(true), nsmgr, 0);
            }
            else
            {
                List<XmlNode> lx = new List<XmlNode>();
                for (int c = 0; c < this.Length; c++)
                {
                    lx.Add(this[c].CloneNode(true));
                }

                XmlOperV2 r = XmlOperV2.load(lx, nsmgr, 0);
                r.position = -1;
                return r;
            }
        }

        /// <summary>
        /// 替换节点，多节点替换多节点要求数量一致！
        /// 单节点替换单节点，多节点替换单节点，单节点替换多节点无要求。
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public XmlOperV2 Replace(XmlOperV2 op)
        {
            XmlNode old = this[0];

            //替换根节点
            if (this.initObj.GetType() == typeof(XmlDocument))
            {
                if (op.Length == 1)
                {
                    XmlNode newNode = old.OwnerDocument.ImportNode(op[0], true);
                    this.xmlDoc.RemoveAll();
                    this.xmlDoc.AppendChild(newNode);
                } else {
                    throw new Exception("替换根节点，只能用单节点！");  
                }
            }
            else
            {
                if (old.ParentNode != null) //有些 clone 出来的 XmlNode 是不得 Parent 的。
                {
                    if (this.Length == 1 && op.Length == 1)             //单节点换单节点
                    {
                        XmlNode newNode = old.OwnerDocument.ImportNode(op[0], true);
                        old.ParentNode.ReplaceChild(newNode, old);
                    }
                    else if (this.Length == 1 && op.Length > 1) {       //多节点换单节点
                        _each(op.initObj, delegate(XmlNode node)
                        {
                            XmlNode newNode = old.OwnerDocument.ImportNode(node, true);
                            old.ParentNode.InsertBefore(newNode, old);
                            return true;
                        });
                        old.ParentNode.RemoveChild(old);
                    }
                    else if (this.Length > 1 && op.Length == 1)       //单节点换多节点
                    {   
                        for (var c = 0; c < this.Length; c++)
                        {
                            XmlNode newNode = old.OwnerDocument.ImportNode(op[0], true);
                            this[c].ParentNode.ReplaceChild(newNode, this[c]);
                        }
                    }
                    else
                    {                                                   //多节点换多节点
                        if (this.Length == op.Length)
                        {
                            for (var c = 0; c < this.Length; c++)
                            {
                                XmlNode newNode = old.OwnerDocument.ImportNode(op[c], true);
                                this[c].ParentNode.ReplaceChild(newNode, this[c]);
                            }
                        }
                        else
                        {
                            throw new Exception("节点数组和被替换的数量不一致！");
                        }
                    }
                } else {
                    initObj = op.initObj;  //直接赋值得了
                    xmlDoc = op.xmlDoc;
                }
            }

            return this;
        }

        /// <summary>
        /// 在其后插入节点
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public XmlOperV2 InsertAfter(XmlOperV2 op)
        {
            XmlNode src = this[0];
            XmlNode targ = op[0];

            XmlNode newNode = src.OwnerDocument.ImportNode(targ, true);

            if (src.ParentNode != null)
            {
                src.ParentNode.InsertAfter(newNode, src);
            }
            else
            {
                throw new Exception("必须要有父节点");
            }

            return this;
        }

        /// <summary>
        /// 在其后插入节点
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public XmlOperV2 InsertBefore(XmlOperV2 op)
        {
            XmlNode src = this[0];
            XmlNode targ = op[0];

            XmlNode newNode = src.OwnerDocument.ImportNode(targ, true);

            if (src.ParentNode != null)
            {
                src.ParentNode.InsertBefore(newNode, src);
            }
            else
            {
                throw new Exception("必须要有父节点");
            }

            return this;
        }



        public XmlOperV2 Parent()
        {
            return XmlOperV2.load(_first(initObj).ParentNode, nsmgr, 0);
        }

        public XmlOperV2 Top(int n)
        {
            return Between(0, n - 1);
        }

        public XmlOperV2 Between(int start, int end)
        {
            return XmlOperV2.load(_between(initObj, start, end), nsmgr, 0);
        }

        public XmlOperV2 AppendTo(XmlOperV2 pNode)
        {
            pNode.Append(this);
            return this;
        }

        public XmlOperV2 RemoveAttr(string attName)
        {
            _removeAttr(initObj, attName);
            return this;
        }

        public string Attr(string attName)
        {
            return _attr(initObj, attName);
        }

        public XmlOperV2 Attr(string attName, object value)
        {
            _attr(initObj, attName, value);
            return this;
        }

        public XmlOperV2 Save(string filePath)
        {
            FileOper.WriteFile(filePath, OuterXml);
            return this;
        }

        #endregion

        #region 内部函数

        /// <summary>
        /// List<XmlNode
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="xpath">Xpath.</param>
        /// <param name="fuzzySearch">If set to <c>true</c> fuzzy search.</param>
        /// <param name="nsmgr">Nsmgr.</param>
        /// <param name="ignoreCase">If set to <c>true</c> ignore case.</param>
        private static object _find(object obj, string xpath, bool fuzzySearch = false, XmlNamespaceManager nsmgr = null, bool ignoreCase = false, params string[] nsmgrs)
        {
            /*  
             * 发现一个 XPath 的使用问题
             * 假设有 <root><item><name id="1" /></item><item><name id="2" /></item></root>
             * XmlNodeRoot.SelectNodes("item")[1].SelectNodes("//name") 应该返回什么呢？
             * 按我的个人理解，应该是返回 <name id="2" /> 的，但是结果返回的是 <name id="1" /><name id="2" />
             * 所以此处需要使用绝对路径来定位，不能随便加 //
             */
            List<XmlNode> xn = null;
            XmlNodeList xnl = null;

            _each(obj, (o) =>
            {
                if (!fuzzySearch)
                {
                    try
                    {
                        try
                        {
                            xnl = o.SelectNodes(xpath);
                            if (xnl.Count > 0) return true;
                        }
                        catch { }

                        if (nsmgr != null)
                        {
                            xnl = o.SelectNodes(xpath, nsmgr);
                            if (xnl.Count <= 0)
                            {
                                for (int c = 0; c < nsmgrs.Length; c++)
                                {
                                    string[] stemp = nsmgrs[c].Split('=');
                                    if (stemp.Length != 2) throw new Exception("show:格式xmlns=uri,或xmlns:prefix=uri,或prefix=uri");
                                    stemp[0] = stemp[0].Replace("xmlns:", "");
                                    stemp[0] = stemp[0].Replace("xmlns", "");
                                    if (stemp[0] == "") stemp[0] = "xmlnsDefault";
                                    nsmgr.AddNamespace(stemp[0], stemp[1].Replace("\"", ""));
                                }
                                xnl = o.SelectNodes(_makeNSPrefix(xpath), nsmgr);
                            }
                        } 
                    }
                    catch (Exception ex) {
                        if (ex.Message.StartsWith("show:")) throw ex;
                    }
                }
                else
                {
                    if (!xpath.StartsWith("/") && !xpath.StartsWith("./"))
                        xpath = "//" + xpath;

                    if (ignoreCase)
                    {
                        xpath = xpath.ToLower();

                        string newXmlStr = o.OuterXml.ToLower();
                        newXmlStr = (new Regex(@"xmlns[\:]?[^=]*\=\"".*?\""")).Replace(newXmlStr, "");
                        newXmlStr = (new Regex(@"(\s+|<[\/]?)(?:\w+)\:")).Replace(newXmlStr, "$1");
                        XmlNode newXml = XmlOperV2.load(newXmlStr)[0];

                        XmlNodeList _xnl = newXml.SelectNodes(xpath);
                        /* 这个算法有问题，只考虑到了连续的节点，不过算是个思路                   
                        string firstPos = getNodePosition(newXml, _first(xnl), 0); //找到 o2 在它的文档中的位置
                    
                        int _tmp = firstPos.LastIndexOf(",") + 1;
                        int curPos = int.Parse(firstPos.Substring(_tmp, firstPos.Length - _tmp));
                        firstPos = firstPos.Substring(0, _tmp);
                        for (int cpos = 0; cpos < xnl.Count; cpos++) {
                            XmlNode n = getNodeByPosition(o, firstPos + curPos++);
                            xn.Add(n);
                        }*/
                        //Console.WriteLine(xpath + "," + xnl.Count);
                        //Console.WriteLine("=============================================");
                        xn = new List<XmlNode>();
                        _each(_xnl, (o2) =>
                        {
                            //Console.WriteLine(idx++ + "," + xpath + "," + xnl.Count);
                            string pos = _getNodePosition(newXml, o2);
                            xn.Add(_getNodeByPosition(o, pos));

                            return true;
                        });
                    }
                    else
                    {
                        try {
                            if (nsmgr != null)
                                xnl = o.SelectNodes(_makeNSPrefix(xpath), nsmgr);
                            else
                                xnl = o.SelectNodes(xpath);
                        }
                        catch { }
                    }
                }
                if (xnl == null) xnl = o.SelectNodes("/eewfegwqrgwqrgregqwr1234e1341"); //构造一个不存在的节点，避免返回 null;

                return true;
            });

            if (xn != null)
                return xn;
            else
                return xnl;
        }

        private static List<XmlNode> _between(object obj, int n, int m)
        {
            List<XmlNode> lx = new List<XmlNode>();
            int idx = 0;
            _each(obj, (o) =>
            {
                if (idx++ < n)
                    return true;
                lx.Add(o);
                if (idx > m)
                    return false;
                return true;
            });
            return lx;
        }

        private static void _each(object obj, EachHandle2 eachCallBack)
        {
            switch (obj.GetType().Name)
            {
                case "XmlDocument":
                    if (eachCallBack != null)
                    {
                        eachCallBack(((XmlDocument)obj).DocumentElement);
                    }
                    break;
                case "XmlNode":
                case "XmlElement":
                    if (eachCallBack != null)
                    {
                        eachCallBack((XmlNode)obj);
                    }
                    break;
                case "XmlNodeList":
                case "XPathNodeList":
                    foreach (XmlNode n in (XmlNodeList)obj)
                    {
                        if (eachCallBack != null)
                        {
                            if (eachCallBack(n) == false)
                                break;
                        }
                    }
                    break;
                default:
                    if (obj.GetType() == typeof(List<XmlNode>))
                    {
                        foreach (XmlNode n in (List<XmlNode>)obj)
                        {
                            if (eachCallBack != null)
                            {
                                if (eachCallBack(n) == false)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("XmlOperV2::_each() 未知类型！");
                    }

                    break;
            }
        }

        private static XmlNode _eq(Object obj, int index)
        {
            int _idx = 0;
            XmlNode ret = null;
            _each(obj, (o) =>
            {
                if (_idx++ == index)
                {
                    ret = o;
                    return false;
                }
                return true;
            });
            return ret;
        }

        private static string _text(object obj)
        {
            XmlNode n = _first(obj);
            if (n == null)
                return "";
            return n.InnerText;
        }

        private static string _xml(object obj)
        {
            XmlNode n = _first(obj);
            if (n == null)
                return "";
            return n.InnerXml;
        }

        private static void _xml(object obj, object val)
        {
            _each(obj, (o) =>
            {
                string v = val.ToString().Trim();
                if (v.StartsWith("<![CDATA[") && v.EndsWith("]]>"))
                {
                    var cdata = o.OwnerDocument.CreateCDataSection(v.Substring(9, v.Length - 12));
                    o.InnerXml = "";
                    o.AppendChild(cdata);
                }
                else
                {
                    o.InnerXml = val.ToString();
                }
                return true;
            });
        }

        private static void _text(object obj, object val)
        {
            _each(obj, (o) =>
            {
                o.InnerText = val.ToString();
                return true;
            });
        }

        private static void _append(object obj, XmlNode node)
        {
            _first(obj).AppendChild(node);
        }

        private static XmlNode _first(Object obj)
        {
            return _eq(obj, 0);
        }

        private static void _removeAttr(Object obj, string attName)
        {
            _each(obj, (o) =>
            {
                ((XmlElement)o).RemoveAttribute(attName);
                return true;
            });
        }

        private static string _attr(Object obj, string attName)
        {
            XmlNode n = _first(obj);
            if (n == null)
                return "";
            return ((XmlElement)n).GetAttribute(attName);
        }

        private static void _attr(Object obj, string attName, object val)
        {
            _each(obj, (o) =>
            {
                ((XmlElement)o).SetAttribute(attName, val.ToString());
                return true;
            });
        }


        private static string _getNodePosition(XmlNode anNode, XmlNode cNode)
        {
            string pos = "";

            if (cNode.ParentNode != anNode)
            {
                pos = _getNodePosition(anNode, cNode.ParentNode);
            }

            for (int c = 0; c < cNode.ParentNode.ChildNodes.Count; c++)
            {
                if (cNode.ParentNode.ChildNodes[c] == cNode)
                {
                    pos = pos + "," + c.ToString();
                    if (pos.StartsWith(","))
                    {
                        pos = pos.Remove(0, 1);
                    }
                    return pos;
                }
            }

            return "-1";
        }

        /// <summary>
        /// 在文档中根据位置寻找节点
        /// </summary>
        /// <returns>The node by position.</returns>
        /// <param name="anNode">顶节点</param>
        /// <param name="iLev">需要寻找的深度</param>
        /// <param name="iIdx">需要寻找的索引</param>
        /// <param name="lev">当前深度</param>
        private static XmlNode _getNodeByPosition(XmlNode anNode, string pos)
        {
            string[] spos = pos.Split(',');
            XmlNode ret = anNode;
            foreach (string idx in spos)
            {
                ret = ret.ChildNodes[int.Parse(idx)];
            }
            return ret;
        }

        private static string _makeXmlTree(string input)
        {
            string[] tags = input.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string xmlStr = "{0}";
            for (int c_tags = 0; c_tags < tags.Length; c_tags++)
            {
                string NodeDesc = tags[c_tags];
                Regex reg = new Regex(@"(\w+)(?:\[([^\]]+)\])?");
                MatchCollection mc = reg.Matches(NodeDesc);
                string strNodes = "";
                foreach (Match m in mc)
                {
                    if (m != null)
                    {
                        string tagName = m.Groups[1].Value;
                        string ext = m.Groups[2].Value;

                        XmlOperV2 xNode = XmlOperV2.load("<${tagName}>{0}</${tagName}>".Replace("${tagName}", tagName));
                        string[] attrs = ext.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        for (int c_attrs = 0; c_attrs < attrs.Length; c_attrs++)
                        {
                            string attName = attrs[c_attrs].Split('=')[0];
                            string attValue = attrs[c_attrs].Split('=')[1];
                            switch (attName)
                            {
                                case "innerText":
                                    xNode.Text(attValue);
                                    break;
                                case "innerXml":
                                    xNode.Xml(attValue);
                                    break;
                                default:
                                    xNode.Attr(attName, attValue);
                                    break;
                            }
                        }

                        strNodes += xNode.OuterXml;
                    }
                }
                xmlStr = string.Format(xmlStr, strNodes);
            }
            return xmlStr.Replace("{0}", "");
        }

        private static string _makeNSPrefix(string xpath)
        {
            Regex reg = new Regex(@"(^|\/)(\w+)(\[|\/|$)");
            xpath = reg.Replace(xpath, @"$1xmlnsDefault:$2$3");
            return xpath;
        }

        private static string[] _getXpathPrefix(string xpath) {
            Regex reg = new Regex(@"(\w*?):\w*");
            MatchCollection mc = reg.Matches(xpath);
            List<string> sRet = new List<string>();
            for (int c = 0; c < mc.Count; c++)
            {
                string prefix = mc[c].Groups[1].Value;
                if (!sRet.Contains(prefix)) {
                    sRet.Add(prefix);
                }
            }
            return sRet.ToArray();
        }

        /// <summary>
        /// 取所有的命名空间
        /// </summary>
        /// <param name="_xmlDoc"></param>
        /// <returns></returns>
        private static XmlNamespaceManager _getNameTable(XmlDocument _xmlDoc, ref List<KeyValuePair<string, string>> lstXmlns)
        {
            XmlNamespaceManager _xnm = null;
            Regex reg = new Regex(@"xmlns\:?(.*?)?=""(.*?)""");
            MatchCollection mc = reg.Matches(_xmlDoc.OuterXml);
            if (mc.Count > 0)
            {
                _xnm = new XmlNamespaceManager(_xmlDoc.NameTable);
                for (int c = 0; c < mc.Count; c++)
                {
                    string n = mc[c].Groups[1].Value;
                    if (n == "")
                        n = "xmlnsDefault";
                    string v = mc[c].Groups[2].Value;

                    lstXmlns.Add(new KeyValuePair<string, string>(n, v));   //记录所有的前缀和命名空间
                    _xnm.AddNamespace(n, v);                                //这个遇到重复的前缀会被覆盖掉
                }
            }
            return _xnm;
        }

        #endregion
    }
}
