| Hex  | Dec   | Opcode                     | Argument Format (Hex)         | Argument(s)                                                                       | Notes |
|:----:|------:|----------------------------|:------------------------------|:----------------------------------------------------------------------------------|-------|
| `00` |   `0` | `Nop`                      |                               |                                                                                   |
| `01` |   `1` | `Null`                     |                               |                                                                                   |
| `02` |   `2` | `Int32One`                 |                               |                                                                                   |
| `03` |   `3` | `Int32Zero`                |                               |                                                                                   |
| `04` |   `4` | `Int8Const`                | `vv`                          | `int8_t value`                                                                    |
| `05` |   `5` | `Int16Const`               | `vv vv`                       | `int16_t value`                                                                   |
| `06` |   `6` | `Int32Const`               | `vv vv vv vv`                 | `int32_t value`                                                                   |
| `07` |   `7` | `Int64Const`               | `vv vv vv vv vv vv vv vv`     | `int64_t value`                                                                   |
| `08` |   `8` | `Uint8Const`               | `vv`                          | `uint8_t value`                                                                   |
| `09` |   `9` | `Uint16Const`              | `vv vv`                       | `uint16_t value`                                                                  |
| `0A` |  `10` | `Uint32Const`              | `vv vv vv vv`                 | `uint32_t value`                                                                  |
| `0B` |  `11` | `Uint64Const`              | `vv vv vv vv vv vv vv vv`     | `uint64_t value`                                                                  |
| `0C` |  `12` | `FloatConst`               | `vv vv vv vv`                 | `float value`                                                                     |
| `0D` |  `13` | `DoubleConst`              | `vv vv vv vv`                 | `double value`                                                                    |
| `0E` |  `14` | `NameConst`                | `nn nn nn nn`                 | `uint32_t name_index`                                                             | Index into name table.
| `0F` |  `15` | `EnumConst`                | `tt tt tt tt` `ll ll ll ll`   | `uint32_t enumeration_def_index, enumeral_def_index`                              |
| `10` |  `16` | `StringConst`              | `ss ss ss ss` `vv[size]`      | `uint32_t size` `char8_t value[size];`                                            |
| `11` |  `17` | `TweakDBIdConst`           | `tt tt tt tt`                 | `uint32_t tweakdbid_index`                                                        | Index into TweakDB Id table.
| `12` |  `18` | `ResourceConst`            | `rr rr rr rr`                 | `uint32_t resource_index`                                                         | Index into resource table.
| `13` |  `19` | `BoolTrue`                 |                               |                                                                                   |
| `14` |  `20` | `BoolFalse`                |                               |                                                                                   |
| `15` |  `21` | Unknown                    |                               |                                                                                   |
| `16` |  `22` | `Assign`                   |                               |                                                                                   |
| `17` |  `23` | `Target`                   |                               |                                                                                   |
| `18` |  `24` | `LocalVar`                 | `ll ll ll ll`                 | `uint32_t local_def_index`                                                        |
| `19` |  `25` | `ParamVar`                 | `pp pp pp pp`                 | `uint32_t parameter_def_index`                                                    |
| `1A` |  `26` | `ObjectVar`                | `pp pp pp pp`                 | `uint32_t property_def_index`                                                     |
| `1B` |  `27` | Unknown                    |                               |                                                                                   | Possibly `Breakpoint`. Only available at runtime.
| `1C` |  `28` | `Switch`                   | `dd dd dd dd` `ff ff`         | `uint32_t definition_index` `int16_t first_case_offset`                           | 
| `1D` |  `29` | `SwitchLabel`              | `ff ff` `tt tt`               | `int16_t false_offset` `int16_t true_offset`                                      |
| `1E` |  `30` | `SwitchDefault`            |                               |                                                                                   |
| `1F` |  `31` | `Jump`                     | `tt tt`                       | `int16_t target_offset`                                                           |
| `20` |  `32` | `JumpIfFalse`              | `tt tt`                       | `int16_t target_offset`                                                           |
| `21` |  `33` | `Skip`                     | `tt tt`                       | `int16_t target_offset`                                                           |
| `22` |  `34` | `Conditional`              | `ff ff` `tt tt`               | `int16_t false_offset` `int16_t true_offset`                                      |
| `23` |  `35` | `Constructor`              | `pp` `cc cc cc cc`            | `uint8_t parameter_count` `uint32_t class_def_index`                              |
| `24` |  `36` | `FinalFunc`                | `nn nn` `ss ss` `ff ff ff ff` | `int16_t next_offset` `uint16_t source_line` `uint32_t function_def_index`        |
| `25` |  `37` | `VirtualFunc`              | `nn nn` `ss ss` `nn nn nn nn` | `int16_t next_offset` `uint16_t source_line` `uint32_t function_name_index`       |
| `26` |  `38` | `ParamEnd`                 |                               |                                                                                   |
| `27` |  `39` | `Return`                   |                               |                                                                                   |
| `28` |  `40` | `StructMember`             | `pp pp pp pp`                 | `uint32_t property_def_index`                                                     |
| `29` |  `41` | `Context`                  | `tt tt`                       | `int16_t target_offset`                                                           |
| `2A` |  `42` | `TestEqual`                | `dd dd dd dd`                 | `uint32_t definition_index`                                                       |
| `2B` |  `43` | `TestNotEqual`             | `dd dd dd dd`                 | `uint32_t definition_index`                                                       |
| `2C` |  `44` | `New`                      | `dd dd dd dd`                 | `uint32_t definition_index`                                                       |
| `2D` |  `45` | `Delete`                   |                               |                                                                                   |
| `2E` |  `46` | `This`                     |                               |                                                                                   |
| `2F` |  `47` | Unknown                    | `ss ss ss ss` `bb[size]`      | `uint32_t size` `uint8_t bytes[size]`                                             |
| `30` |  `48` | `ArrayClear`               | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `31` |  `49` | `ArraySize`                | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `32` |  `50` | `ArrayResize`              | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `33` |  `51` | `ArrayFindFirst`           | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `34` |  `52` | `ArrayFindFirstFast`       | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `35` |  `53` | `ArrayFindLast`            | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `36` |  `54` | `ArrayFindLastFast`        | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `37` |  `55` | `ArrayContains`            | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `38` |  `56` | `ArrayContainsFast`        | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `39` |  `57` | Unknown (array related)    | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3A` |  `58` | Unknown (array related)    | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3B` |  `59` | `ArrayPushBack`            | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3C` |  `60` | `ArrayPopBack`             | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3D` |  `61` | `ArrayInsert`              | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3E` |  `62` | `ArrayRemove`              | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `3F` |  `63` | `ArrayRemoveFast`          | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `40` |  `64` | `ArrayGrow`                | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `41` |  `65` | `ArrayErase`               | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `42` |  `66` | `ArrayEraseFast`           | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `43` |  `67` | `ArrayLast`                | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `44` |  `68` | `ArrayElement`             | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `45` |  `69` | `StaticArraySize`          | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `46` |  `70` | `StaticArrayFindFirst`     | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `47` |  `71` | `StaticArrayFindFirstFast` | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `48` |  `72` | `StaticArrayFindLast`      | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `49` |  `73` | `StaticArrayFindLastFast`  | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4A` |  `74` | `StaticArrayContains`      | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4B` |  `75` | `StaticArrayContainsFast`  | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4C` |  `76` | Unknown (array related)    | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4D` |  `77` | Unknown (array related)    | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4E` |  `78` | `StaticArrayLast`          | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `4F` |  `79` | `StaticArrayElement`       | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `50` |  `80` | `HandleToBool`             |                               |                                                                                   |
| `51` |  `81` | `WeakHandleToBool`         |                               |                                                                                   |
| `52` |  `82` | `EnumToInt32`              | `nn nn nn nn` `ss`            | `uint32_t native_def_index` `uint8_t size`                                        |
| `53` |  `83` | `Int32ToEnum`              | `nn nn nn nn` `ss`            | `uint32_t native_def_index` `uint8_t size`                                        |
| `54` |  `84` | `DynamicCast`              | `cc cc cc cc` `uu`            | `uint32_t class_def_index` `uint8_t unknown`                                      |
| `55` |  `85` | `ToString`                 | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `56` |  `86` | `ToVariant`                | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `57` |  `87` | `FromVariant`              | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `58` |  `88` | `VariantIsValid`           |                               |                                                                                   |
| `59` |  `89` | `VariantIsHandle`          |                               |                                                                                   |
| `5A` |  `90` | `VariantIsArray`           |                               |                                                                                   |
| `5B` |  `91` | Unknown                    |                               |                                                                                   |
| `5C` |  `92` | `VariantToString`          |                               |                                                                                   |
| `5D` |  `93` | `WeakHandleToHandle`       |                               |                                                                                   |
| `5E` |  `94` | `HandleToWeakHandle`       |                               |                                                                                   |
| `5F` |  `95` | `WeakHandleNull`           |                               |                                                                                   |
| `60` |  `96` | `ToScriptRef`              | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `61` |  `97` | `FromScriptRef`            | `nn nn nn nn`                 | `uint32_t native_def_index`                                                       |
| `62` |  `98` | Unknown                    |                               |                                                                                   |
