﻿# UL
Universe Language used for all platforms,Can be converted to any other language

UL语言本身不是一种语言，而是一套标准和工具集。

![编辑器预览](https://github.com/xiongfang/UL/blob/master/Documents/editor.png "编辑器预览")

# 当前进度

目前正在维护的插件是C#转C++的插件。

目前支持类，结构体，泛型，枚举的转化。
支持除了lock语句之外的所有语法类型。

类成员支持变量，成员方法。
下一步将实现反射，事件，委托。

标准库方面，已经支持了系统库的基本类型，包括Byte,SByte,Char,Int16,Int32,Int64,UInt16,UInt32,UInt64,Single,Double,Array。

c++对象的垃圾收集器目前使用的是引用计数方式，有循环引用问题，后面需要增加标记清除法。


# 设计概述
整套系统包含4个部分：
- 1.用JSON文件格式表示的代码源文件。源码用JSON格式表示，可以很方便的访问所有类，成员方法，成员函数，以及函数的实现代码，利用插件可以将此格式的源码转为可读性更好的语言的源码，例如C#。
- 2.代码转化器插件：每个插件都可以将一种类型的语言转化为JSON格式,并将JSON格式的源文件转化为指定的语言。理论上，JSON文件的源代码可以转化为任意语言，甚至自己实现的虚拟机语言。甚至直接编译成机器码可执行程序。
- 3.标准库文件。基于运行时库构建的预定义的标准库，JSON文件表示。（目前以.Net 2.0标准库为参考）
- 4.最小目标平台接口（非必要）。只要实现这套运行时接口，则程序可以在目标平台运行。

# 设计目的
- 代码跨语言

  通过转化器插件，用一种语言写的代码，可以转化为任意语言，这能让各种分别擅长不同语言的程序员合作编程。
- 真跨平台

  首先，由于每个平台都有独自的语言，因此，得益于代码的跨平台，设计的程序，利用代码转化插件，可以编译成任意目标平台的代码。此跨平台不同于java，C#的跨平台之处在于，java,C#之类的语言，是虚拟机运行时跨平台，代码需要在虚拟机中执行。而UL，则是生成程序的跨平台，因为此代码可以转为目标平台支持的语言，例如C++可以在大部分平台上编译。
- 高开发效率

  UL语言首先解决了各自熟悉不同语言的程序员之间的鸿沟，减少程序员的学习成本。UL语言设计的指导原则是，工具能够帮程序员做的事情，决不让程序员去做，而交给插件去做，程序员只要关注于业务逻辑，而不需要进入到转化编译的细节。UL语言本身集多种语言的长处，例如：自动垃圾回收机制
- 高执行效率

  相对于java，C#等语言，由于UL代码可以编译成经过优化的目标平台程序，因此UL程序理论上执行效率更高。
- 高可优化性

  理论上，只需要实现目标平台的运行时库，代码就可以在目标平台运行，这提供了足够简单的模式。但是，如果对于性能有更加变态的需求，理论上，所有的标准库函数都可以特殊实现，甚至硬件实现。

# 用例

此工具多钟情况下可用，此处举例说明，但不包含所有用例：

- 跨语言跨平台合作开发

  UL语言的特性，使跨语言跨平台合作开发成为可能
- 用来当作脚本解释执行

  JSON格式表示的源码，方便的读取和访问。使编译器和虚拟机的实现变得非常简单。首先编译器不再需要了，只需要一个读取JSON文件，就可以获得所有元数据。以及函数实现代码的优化表示结构。只需要实现简单的几个接口，则可以实现一个虚拟机。

# LICENSE
- Licensed under the MIT license

- see [LICENSE](https://github.com/xiongfang/UL/blob/master/LICENSE) for details
