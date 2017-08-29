This class supply a method to visit XML file, XML string, XMLDocment..
It's sample use like JQuery.
You just need add a line 'XmlOperV2.LoadHandle x = ((o) =&gt; { return XmlOperV2.load(o); });' in your own class.
and now you can use it.

eg:<br>
var xmlfile1 = x("filepath....");<br>
xmlfile1.Find("//NodeName").Text("abc");    //change Node Text<br>

xmlfile1.Find("//NodeName").Xml("abc");     //change Node Xml<br>

var result1 = xmlfile1.Find("//NodeName").Text(); //get Node Text<br>

var result2 = xmlfile1.Find("//NodeName").Xml();  //get Node Xml<br>

xmlfile1.Find("//NodeName").CDATA("&lt;test content&gt;&lt;&gt;");  //set Node CDATA Value<br>
  
  var rootNode = x("root");   //Make a &lt;root&gt;&lt;/root&gt; Node<br>
  
  x("root/item");             //Make a tree node &lt;root&gt;&lt;item /&gt;&lt;/root&gt;<br>
  
  x("root").Append(x("item")); //Make a tree node &lt;root&gt;&lt;item /&gt;&lt;/root&gt;<br>
  
  x("root").Append(x("item").Text("TestValue")); //Make a tree node &lt;root&gt;&lt;item&gt;TestValue&lt;/item&gt;&lt;/root&gt;<br>
  
  x("&lt;root xmlns:a=\"test.com\"&gt;&lt;a:item&gt;testValue&lt;/a:item&gt;&lt;/root&gt;").Find("//a:item").Text();  //get Namespace Node Text.<br>
  
  x("root").Append(x("item").Text("张静")).Find("//item").Replace(x("Item").Text("张静"));  //Replace Node.<br>
  
  
Some advanced method:<br>
x("&lt;root&gt;&lt;item&gt;张静&lt;/item&gt;&lt;item&gt;张桢炫&lt;/item&gt;&lt;/root&gt;").Find("//item").Each(delegate(o) {
  Console.WriteLine(o.Text());
  return true;
});
