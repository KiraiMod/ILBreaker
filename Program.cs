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
    if (method.ReturnType.FullName == "System.Void")
        return;

    var il = method.Body.GetILProcessor();
    var first = method.Body.Instructions[0];
    var last = method.Body.Instructions[^1];

    il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0));
    il.InsertBefore(first, il.Create(OpCodes.Switch, new Instruction[] { first, last }));
}

MemoryStream mem = new();
asm.Write(mem);
asm.Dispose();

File.WriteAllBytes(target, mem.ToArray());
