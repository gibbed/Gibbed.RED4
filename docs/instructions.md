|  Id  | Opcode                     | Format (Hex)                       | Argument(s)                                                                       | Notes |
|:----:|:---------------------------|:-----------------------------------|:----------------------------------------------------------------------------------|-------|
|  `0` | `Nop`                      | `00`                               |                                                                                   |
|  `1` | `Null`                     | `01`                               |                                                                                   |
|  `2` | `Int32One`                 | `02`                               |                                                                                   |
|  `3` | `Int32Zero`                | `03`                               |                                                                                   |
|  `4` | `Int8Const`                | `04` `xx`                          | `int8_t value`                                                                    |
|  `5` | `Int16Const`               | `05` `xx xx`                       | `int16_t value`                                                                   |
|  `6` | `Int32Const`               | `06` `xx xx xx xx`                 | `int32_t value`                                                                   |
|  `7` | `Int64Const`               | `07` `xx xx xx xx xx xx xx xx`     | `int64_t value`                                                                   |
|  `8` | `Uint8Const`               | `08` `xx`                          | `uint8_t value`                                                                   |
|  `9` | `Uint16Const`              | `09` `xx xx`                       | `uint16_t value`                                                                  |
| `10` | `Uint32Const`              | `0A` `xx xx xx xx`                 | `uint32_t value`                                                                  |
| `11` | `Uint64Const`              | `0B` `xx xx xx xx xx xx xx xx`     | `uint64_t value`                                                                  |
| `12` | `FloatConst`               | `0C` `xx xx xx xx`                 | `float value`                                                                     |
| `13` | `DoubleConst`              | `0D` `xx xx xx xx`                 | `double value`                                                                    |
| `14` | `NameConst`                | `0E` `xx xx xx xx`                 | `uint32_t name_index`                                                             | Index into name table.
| `15` | `EnumConst`                | `0F` `xx xx xx xx` `yy yy yy yy`   | `uint32_t enumeration_def_index, enumeral_def_index`                              |
| `16` | `StringConst`              | `10` `xx xx xx xx` `yy[size]`      | `uint32_t size` `char8_t value[size];`                                            |
| `17` | `TweakDBIdConst`           | `11` `xx xx xx xx`                 | `uint32_t tweakdbid_index`                                                        | Index into TweakDB Id table.
| `18` | `ResourceConst`            | `12` `xx xx xx xx`                 | `uint32_t resource_index`                                                         | Index into resource table.
| `19` | `BoolTrue`                 | `13`                               |                                                                                   |
| `20` | `BoolFalse`                | `14`                               |                                                                                   |
| `21` | Unknown                    | `15`                               |                                                                                   |
| `22` | `Assign`                   | `16`                               |                                                                                   |
| `23` | `Target`                   | `17`                               |                                                                                   |
| `24` | `LocalVar`                 | `18` `xx xx xx xx`                 | `uint32_t local_def_index`                                                        |
| `25` | `ParamVar`                 | `19` `xx xx xx xx`                 | `uint32_t parameter_def_index`                                                    |
| `26` | `ObjectVar`                | `1A` `xx xx xx xx`                 | `uint32_t property_def_index`                                                     |
| `27` | Unknown                    | `1B`                               |                                                                                   | Possibly `Breakpoint`. Only available at runtime.
| `28` | `Switch`                   | `1C` `xx xx xx xx` `yy yy`         | `uint32_t definition_index` `int16_t first_case_offset`                           | 
| `29` | `SwitchLabel`              | `1D` `xx xx` `yy yy`               | `int16_t false_offset` `int16_t true_offset`                                      |
| `30` | `SwitchDefault`            | `1E`                               |                                                                                   |
| `31` | `Jump`                     | `1F` `xx xx`                       | `int16_t target_offset`                                                           |
| `32` | `JumpIfFalse`              | `20` `xx xx`                       | `int16_t target_offset`                                                           |
| `33` | `Skip`                     | `21` `xx xx`                       | `int16_t target_offset`                                                           |
| `34` | `Conditional`              | `22` `xx xx` `yy yy`               | `int16_t false_offset` `int16_t true_offset`                                      |
| `35` | `Constructor`              | `23` `xx` `yy yy yy yy`            | `uint8_t parameter_count` `uint32_t class_def_index`                              |
| `36` | `FinalFunc`                | `24` `xx xx` `yy yy` `zz zz zz zz` | `int16_t next_offset` `uint16_t source_line` `uint32_t function_def_index`        |
| `37` | `VirtualFunc`              | `25` `xx xx` `yy yy` `zz zz zz zz` | `int16_t next_offset` `uint16_t source_line` `uint32_t function_name_index`       |
| `38` | `ParamEnd`                 | `26`                               |                                                                                   |
| `39` | `Return`                   | `27`                               |                                                                                   |
| `40` | `StructMember`             | `28` `xx xx xx xx`                 | `uint32_t property_def_index`                                                     |
| `41` | `Context`                  | `29` `xx xx`                       | `int16_t target_offset`                                                           |
| `42` | `TestEqual`                | `2A` `xx xx xx xx`                 | `uint32_t definition_index`                                                       |
| `43` | `TestNotEqual`             | `2B` `xx xx xx xx`                 | `uint32_t definition_index`                                                       |
| `44` | `New`                      | `2C` `xx xx xx xx`                 | `uint32_t definition_index`                                                       |
| `45` | `Delete`                   | `2D`                               |                                                                                   |
| `46` | `This`                     | `2E`                               |                                                                                   |
| `47` | Unknown                    | `2F` `xx xx xx xx` `yy[size]`      | `uint32_t size` `uint8_t bytes[size]`                                             |
| `48` | `ArrayClear`               | `30` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `49` | `ArraySize`                | `31` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `50` | `ArrayResize`              | `32` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `51` | `ArrayFindFirst`           | `33` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `52` | `ArrayFindFirstFast`       | `34` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `53` | `ArrayFindLast`            | `35` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `54` | `ArrayFindLastFast`        | `36` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `55` | `ArrayContains`            | `37` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `56` | `ArrayContainsFast`        | `38` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `57` | Unknown (array related)    | `39` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `58` | Unknown (array related)    | `3A` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `59` | `ArrayPushBack`            | `3B` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `60` | `ArrayPopBack`             | `3C` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `61` | `ArrayInsert`              | `3D` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `62` | `ArrayRemove`              | `3E` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `63` | `ArrayRemoveFast`          | `3F` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `64` | `ArrayGrow`                | `40` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `65` | `ArrayErase`               | `41` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `66` | `ArrayEraseFast`           | `42` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `67` | `ArrayLast`                | `43` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `68` | `ArrayElement`             | `44` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `69` | `StaticArraySize`          | `45` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `70` | `StaticArrayFindFirst`     | `46` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `71` | `StaticArrayFindFirstFast` | `47` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `72` | `StaticArrayFindLast`      | `48` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `73` | `StaticArrayFindLastFast`  | `49` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `74` | `StaticArrayContains`      | `4A` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `75` | `StaticArrayContainsFast`  | `4B` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `76` | Unknown (array related)    | `4C` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `77` | Unknown (array related)    | `4D` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `78` | `StaticArrayLast`          | `4E` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `79` | `StaticArrayElement`       | `4F` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `80` | `HandleToBool`             | `50`                               |                                                                                   |
| `81` | `WeakHandleToBool`         | `51`                               |                                                                                   |
| `82` | `EnumToInt32`              | `52` `xx xx xx xx` `yy`            | `uint32_t native_def_index` `uint8_t size`                                        |
| `83` | `Int32ToEnum`              | `53` `xx xx xx xx` `yy`            | `uint32_t native_def_index` `uint8_t size`                                        |
| `84` | `DynamicCast`              | `54` `xx xx xx xx` `yy`            | `uint32_t class_def_index` `uint8_t unknown`                                      |
| `85` | `ToString`                 | `55` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `86` | `ToVariant`                | `56` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `87` | `FromVariant`              | `57` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `88` | `VariantIsValid`           | `58`                               |                                                                                   |
| `89` | `VariantIsHandle`          | `59`                               |                                                                                   |
| `90` | `VariantIsArray`           | `5A`                               |                                                                                   |
| `91` | Unknown                    | `5B`                               |                                                                                   |
| `92` | `VariantToString`          | `5C`                               |                                                                                   |
| `93` | `WeakHandleToHandle`       | `5D`                               |                                                                                   |
| `94` | `HandleToWeakHandle`       | `5E`                               |                                                                                   |
| `95` | `WeakHandleNull`           | `5F`                               |                                                                                   |
| `96` | `ToScriptRef`              | `60` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `97` | `FromScriptRef`            | `61` `xx xx xx xx`                 | `uint32_t native_def_index`                                                       |
| `98` | Unknown                    | `62`                               |                                                                                   |
