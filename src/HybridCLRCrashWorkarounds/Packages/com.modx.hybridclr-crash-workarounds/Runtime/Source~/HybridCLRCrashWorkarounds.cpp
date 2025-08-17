#include "codegen/il2cpp-codegen.h"
#include "utils/Exception.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Image.h"

class ScriptingClassPtr
{
public:

    ScriptingClassPtr(Il2CppClass *klass)
    {
        Pointer = klass;
    }

    void *Pointer;
};

#if IL2CPP_TARGET_IOS
void* CreateMonoScriptFromScriptingType(ScriptingClassPtr ptr);
#endif

extern "C"
{
    static void GetMonoBehaviourAndScriptableObjectType(Il2CppClass* &monoBehaviour, Il2CppClass* &scriptableObject);
    static int32_t ReadInt32(const uint8_t* &data, int32_t dataLength, int32_t &readLength);
    static const char* ReadString(const uint8_t* &data, int32_t dataLength, int32_t &readLength);
    static void Validate(void *monoScript, Il2CppClass *klass, bool isDebugBuild);

    static std::string sErrorMessage;
    static std::string sExceptionMessage;

#if IL2CPP_TARGET_ANDROID
  #if IL2CPP_TARGET_ARMV7
    static int32_t sCreateMonoScriptFromScriptingTypeSymbolOffset = 0; // ARMv7
  #elif IL2CPP_TARGET_ARM64
    static int32_t sCreateMonoScriptFromScriptingTypeSymbolOffset = 0; // ARM64
  #elif IL2CPP_TARGET_X86
    static int32_t sCreateMonoScriptFromScriptingTypeSymbolOffset = 0; // X86
  #elif IL2CPP_TARGET_X64
    static int32_t sCreateMonoScriptFromScriptingTypeSymbolOffset = 0; // X86_64
  #endif

    void SetCreateMonoScriptFromScriptingTypeSymbolOffset(int32_t symbolOffset)
    {
        sCreateMonoScriptFromScriptingTypeSymbolOffset = symbolOffset;
    }

    int32_t GetCreateMonoScriptFromScriptingTypeSymbolOffset()
    {
        return sCreateMonoScriptFromScriptingTypeSymbolOffset;
    }
#endif

    int32_t CreateMonoScriptsInternal(const uint8_t *data, int32_t dataLength, bool isDebugBuild, int32_t *errorMessageLength, int32_t *exceptionMessageLength)
    {
        int32_t createdCount = 0;

        try
        {
            if (data == nullptr || dataLength < 4 || memcmp(data, "UMSB", 4) != 0)
                il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("", "Invalid data"));

#if IL2CPP_TARGET_ANDROID
            typedef void* (*CreateMonoScriptFromScriptingType)(ScriptingClassPtr ptr);
            Il2CppMethodPointer baseAddress = il2cpp_codegen_resolve_icall("UnityEngine.GameObject::Internal_AddComponentWithType(System.Type)");
            CreateMonoScriptFromScriptingType func = (CreateMonoScriptFromScriptingType)((uint8_t*)baseAddress + sCreateMonoScriptFromScriptingTypeSymbolOffset);
#endif

            Il2CppClass *monoBehaviour, *scriptableObject;
            GetMonoBehaviourAndScriptableObjectType(monoBehaviour, scriptableObject);

            int32_t readLength = 4;
            data += readLength;

            int32_t assemblyCount = ReadInt32(data, dataLength, readLength);
            bool validated = false;
            
            for (int i = 0; i < assemblyCount; i++)
            {
                const char *assemblyName = ReadString(data, dataLength, readLength);

                const Il2CppAssembly *assembly = il2cpp::vm::Assembly::GetLoadedAssembly(assemblyName);
                if (assembly == nullptr)
                {
                    sErrorMessage = "Invalid assembly: ";
                    sErrorMessage += assemblyName;
                    sErrorMessage += "\n";
                }

                int32_t nameSpaceCount = ReadInt32(data, dataLength, readLength);

                for (int j = 0; j < nameSpaceCount; j++)
                {
                    const char *nameSpace = ReadString(data, dataLength, readLength);

                    int32_t classNameCount = ReadInt32(data, dataLength, readLength);

                    for (int k = 0; k < classNameCount; k++)
                    {
                        const char *className = ReadString(data, dataLength, readLength);
                        
                        if (assembly == nullptr)
                            continue;
                        
                        Il2CppClass *klass = il2cpp::vm::Image::ClassFromName(assembly->image, nameSpace, className);

                        if (klass == nullptr ||
                            il2cpp::vm::Class::IsAbstract(klass) ||
                            il2cpp::vm::Class::IsGeneric(klass) ||
                            il2cpp::vm::Class::GetDeclaringType(klass) != nullptr ||
                            (!il2cpp::vm::Class::IsSubclassOf(klass, monoBehaviour, false) &&
                            !il2cpp::vm::Class::IsSubclassOf(klass, scriptableObject, false)))
                        {
                            sErrorMessage += "Invalid type: ";
                            if (nameSpace[0] != 0)
                            {
                                sErrorMessage += nameSpace;
                                sErrorMessage += ".";
                            }

                            sErrorMessage += className;
                            sErrorMessage += "\n";
                            
                            continue;
                        }

#if IL2CPP_TARGET_IOS
                        void *ptr = CreateMonoScriptFromScriptingType(klass);
#elif IL2CPP_TARGET_ANDROID
                        void *ptr = func(klass);
#endif
                        if (!validated)
                        {
                            validated = true;
                            Validate(ptr, klass, isDebugBuild);
                        }
                        
                        createdCount++;
                    }
                }
            }

            IL2CPP_ASSERT(dataLength == readLength);
        } 
        catch (Il2CppExceptionWrapper &e)
        {
            sExceptionMessage = il2cpp::utils::Exception::FormatException(e.ex);
        }
        
        *errorMessageLength = (int32_t)sErrorMessage.length();
        *exceptionMessageLength = (int32_t)sExceptionMessage.length();
        
        return createdCount;
    }
    
    void GetErrorMessageAndClear(uint8_t *buffer)
    {
        memcpy(buffer, sErrorMessage.c_str(), sErrorMessage.length());
        sErrorMessage = "";
    }
    
    void GetExceptionMessageAndClear(uint8_t *buffer)
    {
        memcpy(buffer, sExceptionMessage.c_str(), sExceptionMessage.length());
        sExceptionMessage = "";
    }
    
    static void GetMonoBehaviourAndScriptableObjectType(Il2CppClass* &monoBehaviour, Il2CppClass* &scriptableObject)
    {
        const Il2CppAssembly *assembly = il2cpp::vm::Assembly::GetLoadedAssembly("UnityEngine");

        monoBehaviour = il2cpp::vm::Image::ClassFromName(assembly->image, "UnityEngine", "MonoBehaviour");
        scriptableObject = il2cpp::vm::Image::ClassFromName(assembly->image, "UnityEngine", "ScriptableObject");
    }
    
    static int32_t ReadInt32(const uint8_t* &data, int32_t dataLength, int32_t &readLength)
    {
        if (dataLength - readLength < 4)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("", "Invalid data"));

        int32_t value = il2cpp_unsafe_read_unaligned<int32_t>((void*)data);
        data += 4;
        readLength += 4;

        return value;
    }

    static const char* ReadString(const uint8_t* &data, int32_t dataLength, int32_t &readLength)
    {
        int32_t stringLength = ReadInt32(data, dataLength, readLength);

        if (stringLength <= 0 || dataLength - readLength < stringLength || data[stringLength - 1] != 0)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("", "Invalid data"));

        const char* str = (const char*)data;
        data += stringLength;
        readLength += stringLength;

        return str;
    }
    
    static void Validate(void *monoScript, Il2CppClass *klass, bool isDebugBuild)
    {
        int32_t offset1 = 0;
        int32_t offset2 = 0;

        // 偏移值来自 libunity.so 的 _ZN10MonoScript8GetClassEv
#if HYBRIDCLR_UNITY_2022
  #if IL2CPP_TARGET_IOS
        offset1 = isDebugBuild ? 160 : 128;
        offset2 = 0;
  #elif IL2CPP_TARGET_ANDROID
    #if IL2CPP_TARGET_ARMV7
        offset1 = isDebugBuild ? 108 : 84;
        offset2 = 0;
    #elif IL2CPP_TARGET_ARM64
        offset1 = isDebugBuild ? 160 : 128;
        offset2 = 0;
    #else
        return;
    #endif
  #else
        return;
  #endif
#else
        return;
#endif

        void *ptr = *(void**)((uint8_t*)monoScript + offset1);
        if (ptr == nullptr)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Called wrong function"));

        ptr = *(void**)((uint8_t*)ptr + offset2);
        if (ptr != klass)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Called wrong function"));
    }
}
