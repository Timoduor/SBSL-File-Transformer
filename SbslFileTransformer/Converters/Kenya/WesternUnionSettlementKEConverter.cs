using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.Kenya
{
    public class WesternUnionSettlementKEConverter
    {
        public WesternUnionSettlementKEConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration() { AutodetectSeparators = new char[] { ',', ';', '\t', '|', '#' } }))
                {
                    int countHeader = 0;

                    string computedbaseamnt = "Computed Base Amount";

                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        var value = reader.GetValue(0).ToString();

                        var check = reader.GetValue(1)?.ToString().Trim();

                        if (string.IsNullOrEmpty(check))
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(value) && value != "Number" && list.Count() > 0)
                        {
                            var last = list.Last();

                            last.Col5 = reader.GetValue(1)?.ToString();

                            last.Col6 = reader.GetValue(2)?.ToString();

                            last.Col7 = reader.GetValue(3)?.ToString();

                            last.Col8 = reader.GetValue(4)?.ToString();

                            last.Col9 = reader.GetValue(5)?.ToString();

                            last.Col10 = reader.GetValue(6)?.ToString();

                            last.Col11 = reader.GetValue(7)?.ToString();

                            last.Col12 = reader.GetValue(8)?.ToString();

                            last.Col13 = reader.GetValue(9)?.ToString();

                            last.Col14 = reader.GetValue(10)?.ToString();

                            last.Col15 = reader.GetValue(11)?.ToString();

                            last.Col16 = reader.GetValue(12)?.ToString();

                            last.Col17 = reader.GetValue(13)?.ToString();

                            last.Col18 = reader.GetValue(14)?.ToString().Replace("\n", "");

                            last.Col19 = reader.GetValue(15)?.ToString().Replace("\n", "");

                            last.Col20 = reader.GetValue(16)?.ToString().Replace("\n", "");

                            last.Col21 = reader.GetValue(17)?.ToString().Replace("\n", "");

                            last.Col22 = reader.GetValue(18)?.ToString().Replace("\n", "");

                            last.Col23 = reader.GetValue(19)?.ToString().Replace("\n", "");

                            last.Col24 = reader.GetValue(20)?.ToString().Replace("\n", "");

                            last.Col25 = reader.GetValue(21)?.ToString().Replace("\n", "");

                            last.Col26 = reader.GetValue(22)?.ToString().Replace("\n", "");

                            last.Col27 = reader.GetValue(23)?.ToString().Replace("\n", "");

                            last.Col28 = reader.GetValue(24)?.ToString().Replace("\n", "");

                            last.Col29 = reader.GetValue(25)?.ToString().Replace("\n", "");

                            last.Col30 = reader.GetValue(26)?.ToString().Replace("\n", "");

                            last.Col31 = reader.GetValue(27)?.ToString().Replace("\n", "");

                            last.Col32 = reader.GetValue(28)?.ToString().Replace("\n", "");

                            last.Col33 = reader.GetValue(29)?.ToString().Replace("\n", "");

                            last.Col34 = reader.GetValue(30)?.ToString().Replace("\n", "");

                            last.Col35 = reader.GetValue(31)?.ToString().Replace("\n", "");

                            last.Col36 = reader.GetValue(32)?.ToString().Replace("\n", "");

                            last.Col37 = reader.GetValue(33)?.ToString().Replace("\n", "");

                            last.Col38 = reader.GetValue(34)?.ToString().Replace("\n", "");

                            last.Col39 = reader.GetValue(35)?.ToString().Replace("\n", "");

                            last.Col40 = reader.GetValue(36)?.ToString().Replace("\n", "");

                            last.Col41 = reader.GetValue(37)?.ToString().Replace("\n", "");

                            last.Col42 = reader.GetValue(38)?.ToString().Replace("\n", "");

                            last.Col43 = reader.GetValue(39)?.ToString().Replace("\n", "");

                            last.Col44 = reader.GetValue(40)?.ToString().Replace("\n", "");

                            last.Col45 = reader.GetValue(41)?.ToString().Replace("\n", "");

                            last.Col46 = reader.GetValue(42)?.ToString().Replace("\n", "");

                            last.Col47 = reader.GetValue(43)?.ToString().Replace("\n", "");

                            last.Col48 = reader.GetValue(44)?.ToString().Replace("\n", "");

                            last.Col49 = reader.GetValue(45)?.ToString().Replace("\n", "");

                            last.Col50 = reader.GetValue(46)?.ToString().Replace("\n", "");

                            last.Col51 = reader.GetValue(47)?.ToString().Replace("\n", "");

                            last.Col52 = reader.GetValue(48)?.ToString().Replace("\n", "");

                            last.Col53 = reader.GetValue(49)?.ToString().Replace("\n", "");

                            last.Col54 = reader.GetValue(50)?.ToString().Replace("\n", "");

                            last.Col55 = reader.GetValue(51)?.ToString().Replace("\n", "");

                            last.Col56 = reader.GetValue(52)?.ToString().Replace("\n", "");

                            last.Col57 = reader.GetValue(53)?.ToString().Replace("\n", "");

                            last.Col58 = reader.GetValue(54)?.ToString().Replace("\n", "");

                            last.Col59 = reader.GetValue(55)?.ToString().Replace("\n", "");

                            last.Col60 = reader.GetValue(56)?.ToString().Replace("\n", "");

                            last.Col61 = reader.GetValue(57)?.ToString().Replace("\n", "");

                            last.Col62 = reader.GetValue(58)?.ToString().Replace("\n", "");

                            last.Col63 = reader.GetValue(59)?.ToString().Replace("\n", "");

                            last.Col64 = reader.GetValue(60)?.ToString().Replace("\n", "");

                            last.Col65 = reader.GetValue(61)?.ToString().Replace("\n", "");

                            last.Col66 = reader.GetValue(62)?.ToString().Replace("\n", "");

                            last.Col67 = reader.GetValue(63)?.ToString().Replace("\n", "");

                            last.Col68 = reader.GetValue(64)?.ToString().Replace("\n", "");

                            last.Col69 = reader.GetValue(65)?.ToString().Replace("\n", "");

                            last.Col70 = reader.GetValue(66)?.ToString().Replace("\n", "");

                            last.Col71 = reader.GetValue(67)?.ToString().Replace("\n", "");

                            last.Col72 = reader.GetValue(68)?.ToString().Replace("\n", "");

                            last.Col73 = reader.GetValue(69)?.ToString().Replace("\n", "");

                            last.Col74 = reader.GetValue(70)?.ToString().Replace("\n", "");

                            last.Col75 = reader.GetValue(71)?.ToString().Replace("\n", "");

                            last.Col76 = reader.GetValue(72)?.ToString().Replace("\n", "");

                            last.Col77 = reader.GetValue(73)?.ToString().Replace("\n", "");

                            last.Col78 = reader.GetValue(74)?.ToString().Replace("\n", "");

                            last.Col79 = reader.GetValue(75)?.ToString().Replace("\n", "");

                            last.Col80 = reader.GetValue(76)?.ToString().Replace("\n", "");

                            last.Col81 = reader.GetValue(77)?.ToString().Replace("\n", "");

                            last.Col82 = reader.GetValue(78)?.ToString().Replace("\n", "");

                            last.Col83 = reader.GetValue(79)?.ToString().Replace("\n", "");

                            last.Col84 = reader.GetValue(80)?.ToString().Replace("\n", "");

                            last.Col85 = reader.GetValue(81)?.ToString().Replace("\n", "");

                            last.Col86 = reader.GetValue(82)?.ToString().Replace("\n", "");

                            last.Col87 = reader.GetValue(83)?.ToString().Replace("\n", "");

                            last.Col88 = reader.GetValue(84)?.ToString().Replace("\n", "");

                            last.Col89 = reader.GetValue(85)?.ToString().Replace("\n", "");

                            last.Col90 = reader.GetValue(86)?.ToString().Replace("\n", "");

                            last.Col91 = reader.GetValue(87)?.ToString().Replace("\n", "");

                            last.Col92 = reader.GetValue(88)?.ToString().Replace("\n", "");

                            last.Col93 = reader.GetValue(89)?.ToString().Replace("\n", "");

                            last.Col94 = reader.GetValue(90)?.ToString().Replace("\n", "");

                            last.Col95 = reader.GetValue(91)?.ToString().Replace("\n", "");

                            last.Col96 = reader.GetValue(92)?.ToString().Replace("\n", "");

                            last.Col97 = reader.GetValue(93)?.ToString().Replace("\n", "");

                            last.Col98 = reader.GetValue(94)?.ToString().Replace("\n", "");

                            last.Col99 = reader.GetValue(95)?.ToString().Replace("\n", "");

                            last.Col100 = reader.GetValue(96)?.ToString().Replace("\n", "");

                            last.Col101 = reader.GetValue(97)?.ToString().Replace("\n", "");

                            last.Col102 = reader.GetValue(98)?.ToString().Replace("\n", "");

                            last.Col103 = reader.GetValue(99)?.ToString().Replace("\n", "");

                            last.Col104 = reader.GetValue(100)?.ToString().Replace("\n", "");

                            last.Col105 = reader.GetValue(101)?.ToString().Replace("\n", "");

                            last.Col106 = reader.GetValue(102)?.ToString().Replace("\n", "");

                            last.Col107 = reader.GetValue(103)?.ToString().Replace("\n", "");

                            last.Col108 = reader.GetValue(104)?.ToString().Replace("\n", "");

                            last.Col109 = reader.GetValue(105)?.ToString().Replace("\n", "");

                            last.Col110 = reader.GetValue(106)?.ToString().Replace("\n", "");

                            last.Col111 = reader.GetValue(107)?.ToString().Replace("\n", "");

                            last.Col112 = reader.GetValue(108)?.ToString().Replace("\n", "");

                            last.Col113 = reader.GetValue(109)?.ToString().Replace("\n", "");

                            last.Col114 = reader.GetValue(110)?.ToString().Replace("\n", "");

                            last.Col115 = reader.GetValue(111)?.ToString().Replace("\n", "");

                            last.Col116 = reader.GetValue(112)?.ToString().Replace("\n", "");

                            last.Col117 = reader.GetValue(112)?.ToString().Replace("\n", "");

                            last.Col118 = reader.GetValue(114)?.ToString().Replace("\n", "");

                            last.Col119 = reader.GetValue(115)?.ToString().Replace("\n", "");

                            last.Col120 = reader.GetValue(116)?.ToString().Replace("\n", "");

                            last.Col121 = reader.GetValue(117)?.ToString().Replace("\n", "");

                            last.Col122 = reader.GetValue(118)?.ToString().Replace("\n", "");

                            last.Col123 = reader.GetValue(119)?.ToString().Replace("\n", "");

                            last.Col124 = reader.GetValue(120)?.ToString().Replace("\n", "");

                            last.Col125 = reader.GetValue(121)?.ToString().Replace("\n", "");

                            last.Col126 = reader.GetValue(122)?.ToString().Replace("\n", "");

                            last.Col127 = reader.GetValue(123)?.ToString().Replace("\n", "");

                            last.Col128 = reader.GetValue(124)?.ToString().Replace("\n", "");

                            last.Col129 = reader.GetValue(125)?.ToString().Replace("\n", "");

                            last.Col130 = reader.GetValue(126)?.ToString().Replace("\n", "");

                            last.Col131 = reader.GetValue(127)?.ToString().Replace("\n", "");

                            last.Col132 = reader.GetValue(128)?.ToString().Replace("\n", "");

                            last.Col133 = reader.GetValue(129)?.ToString().Replace("\n", "");

                            last.Col134 = reader.GetValue(130)?.ToString().Replace("\n", "");

                            last.Col135 = reader.GetValue(131)?.ToString().Replace("\n", "");

                            last.Col136 = reader.GetValue(132)?.ToString().Replace("\n", "");

                            last.Col137 = reader.GetValue(133)?.ToString().Replace("\n", "");

                            last.Col138 = reader.GetValue(134)?.ToString().Replace("\n", "");

                            last.Col139 = reader.GetValue(135)?.ToString().Replace("\n", "");

                            last.Col140 = reader.GetValue(136)?.ToString().Replace("\n", "");

                            last.Col141 = reader.GetValue(137)?.ToString().Replace("\n", "");

                            last.Col142 = reader.GetValue(138)?.ToString().Replace("\n", "");

                            last.Col143 = reader.GetValue(139)?.ToString().Replace("\n", "");

                            last.Col144 = reader.GetValue(140)?.ToString().Replace("\n", "");

                            last.Col145 = reader.GetValue(141)?.ToString().Replace("\n", "");

                            last.Col146 = reader.GetValue(142)?.ToString().Replace("\n", "");

                            last.Col147 = reader.GetValue(143)?.ToString().Replace("\n", "");

                            last.Col148 = reader.GetValue(144)?.ToString().Replace("\n", "");

                            last.Col149 = reader.GetValue(145)?.ToString().Replace("\n", "");

                            last.Col150 = reader.GetValue(146)?.ToString().Replace("\n", "");

                            last.Col151 = reader.GetValue(147)?.ToString().Replace("\n", "");

                            last.Col152 = reader.GetValue(148)?.ToString().Replace("\n", "");

                            last.Col153 = reader.GetValue(149)?.ToString().Replace("\n", "");

                            last.Col154 = reader.GetValue(150)?.ToString().Replace("\n", "");

                            last.Col155 = reader.GetValue(151)?.ToString().Replace("\n", "");

                            last.Col156 = reader.GetValue(152)?.ToString().Replace("\n", "");

                            last.Col157 = reader.GetValue(153)?.ToString().Replace("\n", "");

                            last.Col158 = reader.GetValue(154)?.ToString().Replace("\n", "");

                            last.Col159 = reader.GetValue(155)?.ToString().Replace("\n", "");

                            last.Col160 = reader.GetValue(156)?.ToString().Replace("\n", "");

                            last.Col161 = reader.GetValue(157)?.ToString().Replace("\n", "");

                            last.Col162 = reader.GetValue(158)?.ToString().Replace("\n", "");

                            last.Col163 = reader.GetValue(159)?.ToString().Replace("\n", "");

                            last.Col164 = reader.GetValue(160)?.ToString().Replace("\n", "");

                            last.Col165 = reader.GetValue(161)?.ToString().Replace("\n", "");

                            last.Col166 = reader.GetValue(162)?.ToString().Replace("\n", "");

                            last.Col167 = reader.GetValue(163)?.ToString().Replace("\n", "");

                            last.Col168 = reader.GetValue(164)?.ToString().Replace("\n", "");

                            last.Col169 = reader.GetValue(165)?.ToString().Replace("\n", "");

                            last.Col170 = reader.GetValue(166)?.ToString().Replace("\n", "");

                            last.Col171 = reader.GetValue(167)?.ToString().Replace("\n", "");

                            last.Col172 = reader.GetValue(168)?.ToString().Replace("\n", "");

                            last.Col173 = reader.GetValue(169)?.ToString().Replace("\n", "");

                            last.Col174 = reader.GetValue(170)?.ToString().Replace("\n", "");

                            last.Col175 = reader.GetValue(171)?.ToString().Replace("\n", "");

                            last.Col176 = reader.GetValue(172)?.ToString().Replace("\n", "");

                            last.Col177 = reader.GetValue(173)?.ToString().Replace("\n", "");

                            last.Col178 = reader.GetValue(174)?.ToString().Replace("\n", "");

                            last.Col179 = reader.GetValue(175)?.ToString().Replace("\n", "");

                            last.Col180 = reader.GetValue(176)?.ToString().Replace("\n", "");

                            last.Col181 = reader.GetValue(177)?.ToString().Replace("\n", "");

                            last.Col182 = reader.GetValue(178)?.ToString().Replace("\n", "");

                            last.Col183 = reader.GetValue(179)?.ToString().Replace("\n", "");

                            last.Col184 = reader.GetValue(180)?.ToString().Replace("\n", "");

                            last.Col185 = reader.GetValue(181)?.ToString().Replace("\n", "");

                            last.Col186 = reader.GetValue(182)?.ToString().Replace("\n", "");

                            last.Col187 = reader.GetValue(183)?.ToString().Replace("\n", "");
                            try
                            {
                                double recamnt = Convert.ToDouble(reader.GetValue(90));

                                double totalchamnt = Convert.ToDouble(reader.GetValue(91));

                                if (reader.GetValue(46) != null && reader.GetValue(46).ToString() == "S")
                                {
                                    last.Col188 = (recamnt + totalchamnt).ToString().TrimStart().TrimEnd();
                                }
                                else if (reader.GetValue(46) != null && reader.GetValue(46).ToString() == "P")
                                {
                                    last.Col188 = reader.GetValue(138)?.ToString().TrimStart().TrimEnd();
                                }
                            }
                            catch (Exception)
                            {

                            }
                            continue;
                        }

                        //number
                        row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "");
                        //ContinentName
                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //PayCountry
                        row.Col2 = reader.GetValue(2)?.ToString().Replace("\n", "");
                        //RecCountry
                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "");
                        //LocationName
                        row.Col4 = reader.GetValue(4)?.ToString().Replace("\n", "").TrimEnd().TrimStart();
                        //Address
                        row.Col5 = reader.GetValue(5)?.ToString().Replace("\n", "");
                        //PhoneNumber
                        row.Col6 = reader.GetValue(6)?.ToString().Replace("\n", "");
                        //FaxNumber
                        row.Col7 = reader.GetValue(7)?.ToString().Replace("\n", "");
                        //PC_Equipped
                        row.Col8 = reader.GetValue(8)?.ToString().Replace("\n", "");
                        //PC_Quantity
                        row.Col9 = reader.GetValue(9)?.ToString().Replace("\n", "");
                        //ContactName
                        row.Col10 = reader.GetValue(10)?.ToString().Replace("\n", "");
                        //Title
                        row.Col11 = reader.GetValue(11)?.ToString().Replace("\n", "");
                        //Phone1
                        row.Col12 = reader.GetValue(12)?.ToString().Replace("\n", "");
                        //Phone2
                        row.Col13 = reader.GetValue(13)?.ToString().Replace("\n", "");
                        //Phone3
                        row.Col14 = reader.GetValue(14)?.ToString().Replace("\n", "");
                        //Fax1
                        row.Col15 = reader.GetValue(15)?.ToString().Replace("\n", "");
                        //Fax2
                        row.Col16 = reader.GetValue(16)?.ToString().Replace("\n", "");
                        //EMail1
                        row.Col17 = reader.GetValue(17)?.ToString().Replace("\n", "");

                        row.Col18 = reader.GetValue(18)?.ToString().Replace("\n", "");

                        row.Col19 = reader.GetValue(19)?.ToString().Replace("\n", "");

                        row.Col20 = reader.GetValue(20)?.ToString().Replace("\n", "");

                        row.Col21 = reader.GetValue(21)?.ToString().Replace("\n", "");

                        row.Col22 = reader.GetValue(22)?.ToString().Replace("\n", "");

                        row.Col23 = reader.GetValue(23)?.ToString().Replace("\n", "");

                        row.Col24 = reader.GetValue(24)?.ToString().Replace("\n", "");

                        row.Col25 = reader.GetValue(25)?.ToString().Replace("\n", "");

                        row.Col26 = reader.GetValue(27)?.ToString().Replace("\n", "");

                        row.Col27 = reader.GetValue(27)?.ToString().Replace("\n", "");

                        row.Col28 = reader.GetValue(28)?.ToString().Replace("\n", "");

                        row.Col29 = reader.GetValue(29)?.ToString().Replace("\n", "");

                        row.Col30 = reader.GetValue(30)?.ToString().Replace("\n", "");

                        row.Col31 = reader.GetValue(31)?.ToString().Replace("\n", "");

                        row.Col32 = reader.GetValue(32)?.ToString().Replace("\n", "");

                        row.Col33 = reader.GetValue(33)?.ToString().Replace("\n", "");

                        row.Col34 = reader.GetValue(34)?.ToString().Replace("\n", "");

                        row.Col35 = reader.GetValue(35)?.ToString().Replace("\n", "");

                        row.Col36 = reader.GetValue(36)?.ToString().Replace("\n", "");

                        row.Col37 = reader.GetValue(37)?.ToString().Replace("\n", "");

                        row.Col38 = reader.GetValue(38)?.ToString().Replace("\n", "");

                        row.Col39 = reader.GetValue(39)?.ToString().Replace("\n", "");

                        row.Col40 = reader.GetValue(40)?.ToString().Replace("\n", "");

                        row.Col41 = reader.GetValue(41)?.ToString().Replace("\n", "");

                        row.Col42 = reader.GetValue(42)?.ToString().Replace("\n", "");

                        row.Col43 = reader.GetValue(43)?.ToString().Replace("\n", "");

                        row.Col44 = reader.GetValue(44)?.ToString().Replace("\n", "");

                        row.Col45 = reader.GetValue(45)?.ToString().Replace("\n", "");

                        row.Col46 = reader.GetValue(46)?.ToString().Replace("\n", "");

                        row.Col47 = reader.GetValue(47)?.ToString().Replace("\n", "");

                        row.Col48 = reader.GetValue(48)?.ToString().Replace("\n", "");

                        row.Col49 = reader.GetValue(49)?.ToString().Replace("\n", "");

                        row.Col50 = reader.GetValue(50)?.ToString().Replace("\n", "");

                        row.Col51 = reader.GetValue(51)?.ToString().Replace("\n", "");

                        row.Col52 = reader.GetValue(52)?.ToString().Replace("\n", "");

                        row.Col53 = reader.GetValue(53)?.ToString().Replace("\n", "");

                        row.Col54 = reader.GetValue(54)?.ToString().Replace("\n", "");

                        row.Col55 = reader.GetValue(55)?.ToString().Replace("\n", "");

                        row.Col56 = reader.GetValue(56)?.ToString().Replace("\n", "");

                        row.Col57 = reader.GetValue(57)?.ToString().Replace("\n", "");

                        row.Col58 = reader.GetValue(58)?.ToString().Replace("\n", "");

                        row.Col59 = reader.GetValue(59)?.ToString().Replace("\n", "");

                        row.Col60 = reader.GetValue(60)?.ToString().Replace("\n", "");

                        row.Col61 = reader.GetValue(61)?.ToString().Replace("\n", "");

                        row.Col62 = reader.GetValue(60)?.ToString().Replace("\n", "");

                        row.Col63 = reader.GetValue(63)?.ToString().Replace("\n", "");

                        row.Col64 = reader.GetValue(64)?.ToString().Replace("\n", "");

                        row.Col65 = reader.GetValue(65)?.ToString().Replace("\n", "");

                        row.Col66 = reader.GetValue(66)?.ToString().Replace("\n", "");

                        row.Col67 = reader.GetValue(67)?.ToString().Replace("\n", "");

                        row.Col68 = reader.GetValue(68)?.ToString().Replace("\n", "");

                        row.Col69 = reader.GetValue(69)?.ToString().Replace("\n", "");

                        row.Col70 = reader.GetValue(70)?.ToString().Replace("\n", "");

                        row.Col71 = reader.GetValue(71)?.ToString().Replace("\n", "");

                        row.Col72 = reader.GetValue(72)?.ToString().Replace("\n", "");

                        row.Col73 = reader.GetValue(73)?.ToString().Replace("\n", "");

                        row.Col74 = reader.GetValue(74)?.ToString().Replace("\n", "");

                        row.Col75 = reader.GetValue(75)?.ToString().Replace("\n", "");

                        row.Col76 = reader.GetValue(76)?.ToString().Replace("\n", "");

                        row.Col77 = reader.GetValue(77)?.ToString().Replace("\n", "");

                        row.Col78 = reader.GetValue(78)?.ToString().Replace("\n", "");

                        row.Col79 = reader.GetValue(79)?.ToString().Replace("\n", "");

                        row.Col80 = reader.GetValue(80)?.ToString().Replace("\n", "");

                        row.Col81 = reader.GetValue(81)?.ToString().Replace("\n", "");

                        row.Col82 = reader.GetValue(82)?.ToString().Replace("\n", "");

                        row.Col83 = reader.GetValue(83)?.ToString().Replace("\n", "");

                        row.Col84 = reader.GetValue(84)?.ToString().Replace("\n", "");

                        row.Col85 = reader.GetValue(85)?.ToString().Replace("\n", "");

                        row.Col86 = reader.GetValue(86)?.ToString().Replace("\n", "");

                        row.Col87 = reader.GetValue(87)?.ToString().Replace("\n", "");

                        row.Col88 = reader.GetValue(88)?.ToString().Replace("\n", "");

                        row.Col89 = reader.GetValue(89)?.ToString().Replace("\n", "");

                        row.Col90 = reader.GetValue(90)?.ToString().Replace("\n", "");

                        row.Col91 = reader.GetValue(91)?.ToString().Replace("\n", "");

                        row.Col92 = reader.GetValue(92)?.ToString().Replace("\n", "");

                        row.Col93 = reader.GetValue(93)?.ToString().Replace("\n", "");

                        row.Col94 = reader.GetValue(94)?.ToString().Replace("\n", "");

                        row.Col95 = reader.GetValue(95)?.ToString().Replace("\n", "");

                        row.Col96 = reader.GetValue(96)?.ToString().Replace("\n", "");

                        row.Col97 = reader.GetValue(97)?.ToString().Replace("\n", "");

                        row.Col98 = reader.GetValue(98)?.ToString().Replace("\n", "");

                        row.Col99 = reader.GetValue(99)?.ToString().Replace("\n", "");

                        row.Col100 = reader.GetValue(100)?.ToString().Replace("\n", "");

                        row.Col101 = reader.GetValue(101)?.ToString().Replace("\n", "");

                        row.Col102 = reader.GetValue(102)?.ToString().Replace("\n", "");

                        row.Col103 = reader.GetValue(103)?.ToString().Replace("\n", "");

                        row.Col104 = reader.GetValue(104)?.ToString().Replace("\n", "");

                        row.Col105 = reader.GetValue(105)?.ToString().Replace("\n", "");

                        row.Col106 = reader.GetValue(106)?.ToString().Replace("\n", "");

                        row.Col107 = reader.GetValue(107)?.ToString().Replace("\n", "");

                        row.Col108 = reader.GetValue(108)?.ToString().Replace("\n", "");

                        row.Col109 = reader.GetValue(109)?.ToString().Replace("\n", "");

                        row.Col110 = reader.GetValue(110)?.ToString().Replace("\n", "");

                        row.Col111 = reader.GetValue(111)?.ToString().Replace("\n", "");

                        row.Col112 = reader.GetValue(112)?.ToString().Replace("\n", "");

                        row.Col113 = reader.GetValue(113)?.ToString().Replace("\n", "");

                        row.Col114 = reader.GetValue(114)?.ToString().Replace("\n", "");

                        row.Col115 = reader.GetValue(115)?.ToString().Replace("\n", "");

                        row.Col116 = reader.GetValue(116)?.ToString().Replace("\n", "");

                        row.Col117 = reader.GetValue(117)?.ToString().Replace("\n", "");

                        row.Col118 = reader.GetValue(118)?.ToString().Replace("\n", "");

                        row.Col119 = reader.GetValue(119)?.ToString().Replace("\n", "");

                        row.Col120 = reader.GetValue(120)?.ToString().Replace("\n", "");

                        row.Col121 = reader.GetValue(121)?.ToString().Replace("\n", "");

                        row.Col122 = reader.GetValue(122)?.ToString().Replace("\n", "");

                        row.Col123 = reader.GetValue(123)?.ToString().Replace("\n", "");

                        row.Col124 = reader.GetValue(124)?.ToString().Replace("\n", "");

                        row.Col125 = reader.GetValue(125)?.ToString().Replace("\n", "");

                        row.Col126 = reader.GetValue(126)?.ToString().Replace("\n", "");

                        row.Col127 = reader.GetValue(127)?.ToString().Replace("\n", "");

                        row.Col128 = reader.GetValue(128)?.ToString().Replace("\n", "");

                        row.Col129 = reader.GetValue(129)?.ToString().Replace("\n", "");

                        row.Col130 = reader.GetValue(130)?.ToString().Replace("\n", "");

                        row.Col131 = reader.GetValue(131)?.ToString().Replace("\n", "");

                        row.Col132 = reader.GetValue(132)?.ToString().Replace("\n", "");

                        row.Col133 = reader.GetValue(133)?.ToString().Replace("\n", "");

                        row.Col134 = reader.GetValue(134)?.ToString().Replace("\n", "");

                        row.Col135 = reader.GetValue(135)?.ToString().Replace("\n", "");

                        row.Col136 = reader.GetValue(136)?.ToString().Replace("\n", "");

                        row.Col137 = reader.GetValue(137)?.ToString().Replace("\n", "");

                        row.Col138 = reader.GetValue(138)?.ToString().Replace("\n", "");

                        row.Col139 = reader.GetValue(139)?.ToString().Replace("\n", "");

                        row.Col140 = reader.GetValue(140)?.ToString().Replace("\n", "");

                        row.Col141 = reader.GetValue(141)?.ToString().Replace("\n", "");

                        row.Col142 = reader.GetValue(142)?.ToString().Replace("\n", "");

                        row.Col143 = reader.GetValue(143)?.ToString().Replace("\n", "");

                        row.Col144 = reader.GetValue(144)?.ToString().Replace("\n", "");

                        row.Col145 = reader.GetValue(145)?.ToString().Replace("\n", "");

                        row.Col146 = reader.GetValue(146)?.ToString().Replace("\n", "");

                        row.Col147 = reader.GetValue(147)?.ToString().Replace("\n", "");

                        row.Col148 = reader.GetValue(148)?.ToString().Replace("\n", "");

                        row.Col149 = reader.GetValue(149)?.ToString().Replace("\n", "");

                        row.Col150 = reader.GetValue(150)?.ToString().Replace("\n", "");

                        row.Col151 = reader.GetValue(151)?.ToString().Replace("\n", "");

                        row.Col152 = reader.GetValue(152)?.ToString().Replace("\n", "");

                        row.Col153 = reader.GetValue(153)?.ToString().Replace("\n", "");

                        row.Col154 = reader.GetValue(154)?.ToString().Replace("\n", "");

                        row.Col155 = reader.GetValue(155)?.ToString().Replace("\n", "");

                        row.Col156 = reader.GetValue(156)?.ToString().Replace("\n", "");

                        row.Col157 = reader.GetValue(157)?.ToString().Replace("\n", "");

                        row.Col158 = reader.GetValue(158)?.ToString().Replace("\n", "");

                        row.Col159 = reader.GetValue(159)?.ToString().Replace("\n", "");

                        row.Col160 = reader.GetValue(160)?.ToString().Replace("\n", "");

                        row.Col161 = reader.GetValue(161)?.ToString().Replace("\n", "");

                        row.Col162 = reader.GetValue(162)?.ToString().Replace("\n", "");

                        row.Col163 = reader.GetValue(163)?.ToString().Replace("\n", "");

                        row.Col164 = reader.GetValue(164)?.ToString().Replace("\n", "");

                        row.Col165 = reader.GetValue(165)?.ToString().Replace("\n", "");

                        row.Col166 = reader.GetValue(166)?.ToString().Replace("\n", "");

                        row.Col167 = reader.GetValue(167)?.ToString().Replace("\n", "");

                        row.Col168 = reader.GetValue(168)?.ToString().Replace("\n", "");

                        row.Col169 = reader.GetValue(169)?.ToString().Replace("\n", "");

                        row.Col170 = reader.GetValue(170)?.ToString().Replace("\n", "");

                        row.Col171 = reader.GetValue(171)?.ToString().Replace("\n", "");

                        row.Col172 = reader.GetValue(172)?.ToString().Replace("\n", "");

                        row.Col173 = reader.GetValue(173)?.ToString().Replace("\n", "");

                        row.Col174 = reader.GetValue(174)?.ToString().Replace("\n", "");

                        row.Col175 = reader.GetValue(175)?.ToString().Replace("\n", "");

                        row.Col176 = reader.GetValue(176)?.ToString().Replace("\n", "");

                        row.Col177 = reader.GetValue(177)?.ToString().Replace("\n", "");

                        row.Col178 = reader.GetValue(178)?.ToString().Replace("\n", "");

                        row.Col179 = reader.GetValue(179)?.ToString().Replace("\n", "");

                        row.Col180 = reader.GetValue(180)?.ToString().Replace("\n", "");

                        row.Col181 = reader.GetValue(181)?.ToString().Replace("\n", "");

                        row.Col182 = reader.GetValue(182)?.ToString().Replace("\n", "");

                        row.Col183 = reader.GetValue(183)?.ToString().Replace("\n", "");

                        row.Col184 = reader.GetValue(184)?.ToString().Replace("\n", "");

                        row.Col185 = reader.GetValue(185)?.ToString().Replace("\n", "");

                        row.Col186 = reader.GetValue(186)?.ToString().Replace("\n", "");

                        row.Col187 = reader.GetValue(187)?.ToString().Replace("\n", "");

                        if (countHeader == 0)
                        {

                            row.Col188 = computedbaseamnt;
                        }

                        countHeader++;

                        try
                        {
                            double recamnt = Convert.ToDouble(reader.GetValue(94).ToString());

                            double totalchamnt = Convert.ToDouble(reader.GetValue(95).ToString());

                            if (reader.GetValue(50) != null && reader.GetValue(50).ToString() == "S")
                            {

                                row.Col188 = (recamnt + totalchamnt).ToString().TrimStart().TrimEnd();
                            }
                            else
                            {
                                row.Col188 = row.Col142.TrimStart().TrimEnd();
                            }

                        }
                        catch (Exception)
                        {

                        }

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_WUSKE_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}
