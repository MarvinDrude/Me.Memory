# Me.Memory

A collection of ultra-high-performance, span-based, zero-allocation memory structures, custom collections, and data utilities for .NET.

## 📦 NuGet Packages

| Package Name | NuGet Version |
| :--- | :--- |
| **Me.Memory** | [![Nuget](https://img.shields.io/badge/nuget-0A66C2?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/Me.Memory) |
| **Me.Memory.Serialization** | [![Nuget](https://img.shields.io/badge/nuget-0A66C2?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/Me.Memory.Serialization) |

---

## 🛠️ Me.Memory Types Reference

Below is a quick reference guide to the core high-performance types included in the **Me.Memory** core package, organized by category:

### 💾 Memory & Span Management
| Type | Kind | Description |
| :--- | :--- | :--- |
| **MemoryOwner** | `class` | A high-performance disposable wrapper/holder of rented memory from `MemoryPool<T>` implementing `IMemoryOwner<T>`. |
| **SpanOwner** | `ref struct` | A lightweight stack-only wrapper around rented array pool allocations designed for zero-allocation span actions. |
| **TwoSpan** | `struct` | Represents a contiguous logical sequence composed of up to two separate spans, ideal for zero-allocation ring-buffer reads. |
| **PinnedBlockMemoryPool** | `class` | A specialized memory pool that allocates pinned memory blocks to eliminate GC relocation overhead during unmanaged operations (like async I/O). |

### ✍️ High-Performance Writers & Readers
| Type | Kind | Description |
| :--- | :--- | :--- |
| **ByteWriter** | `ref struct` | High-speed, zero-allocation writer for writing raw bytes or primitives directly into a span. |
| **ByteReader** | `ref struct` | High-speed, zero-allocation reader for reading raw bytes or primitives directly from a span. |
| **BufferWriter** | `ref struct` | A custom high-performance buffer writer implementation designed for sequence-building with minimal overhead. |
| **ArrayBuilder** | `struct` | A zero-allocation helper that dynamically builds arrays from rented blocks. |
| **ArrayBuilderResult** | `struct` | Represents the resulting collection/metadata built from an `ArrayBuilder`. |

### 📝 Indented & Code Generation Writers
| Type | Kind | Description |
| :--- | :--- | :--- |
| **TextWriterIndentSlim** | `ref struct` | A lightweight indented writer designed specifically for code generators with minimal allocations. |
| **CodeTextWriter** | `class` | An ultra-optimized, specialized indented writer with highly-efficient string interpolation extensions optimized for building C# source code. |
| **StreamWriterSlim** | `ref struct` | A lightweight, low-overhead stream writer optimized for performance-critical hot paths. |
| **StreamReaderSlim** | `ref struct` | A lightweight, low-overhead stream reader optimized for performance-critical hot paths. |

### 📦 Collections & Buffers
| Type | Kind | Description |
| :--- | :--- | :--- |
| **CircularBuffer** | `class` | A high-performance thread-safe cyclic queue (ring buffer). |
| **CircularBufferSlim** | `struct` | A lightweight, single-threaded high-speed ring buffer. |
| **SequenceArray** | `struct` | An immutable value-type array wrapper providing sequence-based deep structural equality (`IEquatable`). |
| **ObjectPool** | `class` | An open, highly-efficient concurrent object pooling class designed for garbage-free object reusability. |

### 🪙 Bit Manipulation & Packed Data
| Type | Kind | Description |
| :--- | :--- | :--- |
| **PackedBools8** | `struct` | Packs up to 8 boolean flags into a single 1-byte value-type representation. |
| **PackedBools16** | `struct` | Packs up to 16 boolean flags into a single 2-byte value-type representation. |
| **PackedBools32** | `struct` | Packs up to 32 boolean flags into a single 4-byte value-type representation. |
| **PackedBools64** | `struct` | Packs up to 64 boolean flags into a single 8-byte value-type representation. |
| **Flags128** | `struct` | An ultra-fast 128-bit fixed-size value-type bit array. |
| **Flags256** | `struct` | An ultra-fast 256-bit fixed-size value-type bit array. |

### ⚙️ Utilities, Async & Threading
| Type | Kind | Description |
| :--- | :--- | :--- |
| **Result** | `struct` | A zero-allocation implementation of the functional Result pattern containing success state, an optional value, and error info. |
| **WorkPool** | `class` | A highly optimized lightweight thread scheduler designed to minimize scheduling latency and thread overhead. |
| **IoQueue** | `class` | High-speed I/O queue designed for asynchronous work pipeline synchronization. |

---

## ⚡ Me.Memory.Serialization Package

A high-performance, compile-time binary serialization framework for C# 10+. It generates highly-optimized, zero-allocation serializer and deserializer implementations at compile-time with a deeply-cacheable Roslyn Incremental Generator.

### 🌟 Key Features
- **Zero-Allocation**: No boxing, zero garbage collection pressure, and direct memory writing.
- **Reflection-Free**: Highly-optimized C# serializers are generated at compile-time for absolute speed.
- **Zero Manual Registration**: Automatically registers generated serializers at application startup using a C# `[ModuleInitializer]` to configure `SerializerRegistry<T>`.
- **Dynamic Collection Probing**: Dynamically resolves generic standard collections on-demand at runtime.
- **Polymorphism Support**: Full polymorphic hierarchy support via explicit union tags.

### 🏷️ Attributes Reference

* **`[GenerateSerializer]`**  
  Marks a class, struct, or abstract base class for automatic compilation-time serializer generation.
  
* **`[SerializerPosition(int position)]`**  
  Specifies the ordering index of a property in the serialized binary sequence.
  
* **`[SerializerIgnore]`**  
  Instructs the generator to skip this property entirely, bypassing serialization and deserialization.
  
* **`[UseSerializer(typeof(CustomSerializer))]`**  
  Configures a per-property custom serializer. The generator will emit zero-cost direct calls to your custom serializer's static `Write`, `TryRead`, and `CalculateByteLength` methods.
  
* **`[SerializerUnion(int tag, Type derivedType)]`**  
  Declares polymorphic union hierarchies for abstract base classes to enable clean and safe type-tag-based inheritance serialization.