using Mono.Cecil;
using Mono.Cecil.Cil;

if (Environment.GetCommandLineArgs().Length <= 1)
{
    Console.WriteLine("No target specified");
    Console.ReadLine();
}

string target = Environment.GetCommandLineArgs()[1];

var asm = AssemblyDefinition.ReadAssembly(target);
foreach (TypeDefinition type in asm.MainModule.Types)
    ProcessType(type);

void ProcessType(TypeDefinition type)
{
    if (type.HasNestedTypes)
        foreach (TypeDefinition tp in type.NestedTypes)
            ProcessType(tp);

    if (type.HasMethods)
        foreach (MethodDefinition method in type.Methods)
            ProcessMethod(method);
}

void ProcessMethod(MethodDefinition method)
{
    var il = method.Body.GetILProcessor();

    var first = method.Body.Instructions[0];

    Instruction start = Instruction.Create(OpCodes.Ldc_I4_0);
    il.InsertBefore(first, start);

    Instruction invalidDestination;
    if (method.ReturnType.FullName == "System.Void")
        il.InsertBefore(first, invalidDestination = Instruction.Create(OpCodes.Unaligned, (byte)0));
    else invalidDestination = method.Body.Instructions[^1];

    il.InsertAfter(start, il.Create(OpCodes.Switch, new Instruction[] { first, invalidDestination }));
}

MemoryStream mem = new();
asm.Write(mem);
asm.Dispose();

File.WriteAllBytes(target, mem.ToArray());
