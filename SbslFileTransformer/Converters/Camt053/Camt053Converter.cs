using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SbslFileTransformer.Converters.Camt053
{
    public class Camt053Converter
    {
        public void ProcessCamtFile(string file, string outputFolder)
        {
            var xmlInputData = File.ReadAllText(file);

            var xDoc = XDocument.Load(new StringReader(xmlInputData));

            var unwrapped = xDoc.Descendants();
            var node = unwrapped.Where(n => n.Name.LocalName == "BkToCstmrStmt").First();

            var doc = Serializers.Desiarilize<BankToCustomer>(node.ToString());


            var records = new List<ExtractedRecord>();

            foreach (var entry in doc.Stmt.Ntry)
            {
                var rec = new ExtractedRecord
                {
                    MsgId = entry.NtryDtls.TxDtls.Refs.MsgId,
                    AcctSvcrRef = entry.NtryDtls.TxDtls.Refs.AcctSvcrRef,
                    InstrId = entry.NtryDtls.TxDtls.Refs.InstrId,
                    EndToEndId = entry.NtryDtls.TxDtls.Refs.EndToEndId,
                    TxId = entry.NtryDtls.TxDtls.Refs.TxId,
                    Amount = entry.NtryDtls.TxDtls.Amt,
                    EntryReference = entry.NtryRef,

                    CdtDbtInd = entry.CdtDbtInd,
                    Cd = entry.Sts.Cd,
                    Date = entry.ValDt.Dt,
                    PerEntryCd = entry.BkTxCd.Prtry.Cd,
                    BICFI = entry.NtryDtls.TxDtls.RltdPties.InitgPty.Agt.FinInstnId.BICFI,


                    Id = doc.Stmt.Id,
                    AccountNumber = doc.Stmt.Account.Id.Othr.Id,
                    PgNb = doc.Stmt.StmtPgntn.PgNb,
                    LastPgInd = doc.Stmt.StmtPgntn.LastPgInd,
                    ElctrncSeqNb = doc.Stmt.ElctrncSeqNb,
                    CreDtTm = doc.Stmt.CreDtTm,
                    Sum = doc.Stmt.TxsSummry.TtlNtries.Sum,
                    NbOfNtries = doc.Stmt.TxsSummry.TtlNtries.NbOfNtries,
                    AnyBIC = doc.Stmt.Account.Ownr.Id.OrgId.AnyBIC

                };
                records.Add(rec);
            }

            var balance = new List<BalanceExctracted>();
            foreach (var entry in doc.Stmt.Bal)
            {
                var bal = new BalanceExctracted
                {
                    BalanceCd = entry.Tp.CdOrPrtry.Cd,
                    BalanceAmount = entry.Amt,
                    CdtDbtInd = entry.CdtDbtInd,
                    BalanceDate = entry.Dt.Dt,

                    AccountNumber = doc.Stmt.Account.Id.Othr.Id

                };
                balance.Add(bal);
            }

            var camtRecordsFile = $"{DateTime.Now:yyyy_MM_dd_HH:mm:ss}_CamtRecs.csv";
            SaveFiles.SaveToCsv(records, camtRecordsFile);

            var camtBalanceFile = $"{DateTime.Now:yyyy_MM_dd_HH:mm:ss}_CamtBals.csv";
            SaveFiles.SaveToCsv(records, camtRecordsFile);
        }
    }
}
