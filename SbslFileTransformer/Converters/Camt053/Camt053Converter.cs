using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SbslFileTransformer.Converters.Camt053
{
    public class Camt053Converter
    {
        public void ProcessCamtFile(string file, string outputFolder = null)
        {

            var xmlInputData = File.ReadAllText(file);

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(file);
            }

            var xDoc = XDocument.Load(new StringReader(xmlInputData));

            xDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in xDoc.Descendants())
                elem.Name = elem.Name.LocalName;

            var unwrapped = xDoc.Descendants();
            var node = unwrapped.Where(n => n.Name.LocalName == "Document").First();

            var doc = Serializers.Desiarilize<Document>(node.ToString());


            var records = new List<ExtractedRecord>();

            foreach (var entry in doc.BkStmt.Stmt.Ntry)
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


                    Id = doc.BkStmt.Stmt.Id,
                    AccountNumber = doc.BkStmt.Stmt.Account.Id.Othr.Id,
                    PgNb = doc.BkStmt.Stmt.StmtPgntn.PgNb,
                    LastPgInd = doc.BkStmt.Stmt.StmtPgntn.LastPgInd,
                    ElctrncSeqNb = doc.BkStmt.Stmt.ElctrncSeqNb,
                    CreDtTm = doc.BkStmt.Stmt.CreDtTm,
                    Sum = doc.BkStmt.Stmt.TxsSummry?.TtlNtries?.Sum,
                    NbOfNtries = doc.BkStmt.Stmt.TxsSummry?.TtlNtries?.NbOfNtries,
                    AnyBIC = doc.BkStmt.Stmt.Account.Ownr.Id.OrgId.AnyBIC

                };
                records.Add(rec);
            }

            var balances = new List<BalanceExctracted>();
            foreach (var entry in doc.BkStmt.Stmt.Bal)
            {
                var bal = new BalanceExctracted
                {
                    BalanceCd = entry.Tp.CdOrPrtry.Cd,
                    BalanceAmount = entry.Amt,
                    CdtDbtInd = entry.CdtDbtInd,
                    BalanceDate = entry.Dt.Dt,

                    AccountNumber = doc.BkStmt.Stmt.Account.Id.Othr.Id
                };
                balances.Add(bal);
            }

            var outputRecs = Path.Combine(outputFolder, "Recs");
            Directory.CreateDirectory(outputRecs);

            var camtRecordsFile = Path.Combine(outputRecs, $"{Path.GetFileNameWithoutExtension(file)}.csv");
            SaveFiles.SaveToCsv(records, camtRecordsFile);

            var outputBals = Path.Combine(outputFolder, "Bals");
            Directory.CreateDirectory(outputBals);

            var camtBalanceFile = Path.Combine(outputBals, $"{Path.GetFileNameWithoutExtension(file)}.csv");
            SaveFiles.BalanceToCSV(balances, camtBalanceFile);

        }
    }
}
