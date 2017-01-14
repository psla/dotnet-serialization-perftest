namespace csharp Test.Thrift

/* 
 http://www.markhneedham.com/blog/2008/08/29/c-thrift-examples/
 thrift -r --gen csharp .\BiggerObjectWithBytesThrift.thrift
*/

struct BiggerObjectWithBytesThrift {
	10: string Uri;
	20: string Header;
	30: binary Content;
}