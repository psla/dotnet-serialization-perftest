namespace csharp Test.Thrift

/* 
 http://www.markhneedham.com/blog/2008/08/29/c-thrift-examples/
 thrift -r --gen csharp .\SmallObjectWithStringsThrift.thrift
*/

struct SmallObjectWithStringsThrift {
	10: string String1;
	20: string String2;
	30: string String3;
	40: string String4;
}