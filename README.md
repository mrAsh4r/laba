# **ЛАБОРАТОРНАЯ РАБОТА №1**

### **ЗАДАНИЕ**

Разработать два консольных приложение (клиент и сервер) на языке программирования C# способных обмениваться через канал (pipe) данными в обе стороны. После отправки данных из первого консольного приложения (сервера) требуется дождаться ответа от второго консольного приложения (клиента). В процессе разработки приложений потребуется воспользоваться некоторыми из следующих классов:
*   [NamedPipeClientStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeclientstream?view=net-7.0);
*   [NamedPipeServerStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream?view=net-7.0);
*   [Unsafe](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafe?view=net-7.0);
*   [MemoryMarshal](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.memorymarshal?view=net-7.0);
*   [Span](https://learn.microsoft.com/ru-ru/dotnet/api/system.span-1?view=net-7.0);
*   [ReadOnlySpan](https://learn.microsoft.com/ru-ru/dotnet/api/system.readonlyspan-1?view=net-7.0);

**Механизмы по работе с памятью предоставляемые классами Marshal, NativeLibrary, Encoding, BinaryWriter, BinaryReader, StreamWriter и StreamReader использовать запрещено.**

В качестве передаваемых данных должна использоваться пользовательская структура данных ([struct](https://learn.microsoft.com/ru-ru/dotnet/csharp/language-reference/builtin-types/struct)) содержащая в себе не менее двух полей ([свойств](https://learn.microsoft.com/ru-ru/dotnet/csharp/properties)). Передаваемые данные должны выводиться в консоль для проверки корректности передачи.
