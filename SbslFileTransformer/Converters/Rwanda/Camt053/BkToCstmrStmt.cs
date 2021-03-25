using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{


    [XmlRoot(ElementName = "Document")]
    public class Document
    {

        [XmlElement("BkToCstmrStmt")]
        public BankToCustomer BkStmt { get; set; }

        internal XmlNodeList GetElementsByTagName(string v)
        {
            throw new NotImplementedException();
        }
    }

    [XmlRoot(ElementName = "BkToCstmrStmt")]
    public class BankToCustomer
    {
        [XmlElement("Stmt")]
        public Statement Stmt { get; set; }

    }

    [XmlRoot(ElementName = "Stmt")]
    public class Statement
    {

        [XmlElement("Id")]
        public string Id { get; set; }
        [XmlElement("StmtPgntn")]
        public StatementPage StmtPgntn { get; set; }

        [XmlElement("Acct")]
        public Account Account { get; set; }
        public string ElctrncSeqNb { get; set; }

        public string CreDtTm { get; set; }

        [XmlElement("Bal")]
        public List<Balance> Bal { get; set; }

        [XmlElement("TxsSummry")]
        public TextSummary TxsSummry { get; set; }

        [XmlElement("Ntry")]
        public List<NumberEntry> Ntry { get; set; }

    }

    [XmlRoot(ElementName = "StmtPgntn")]
    public class StatementPage
    {
        [XmlElement("PgNb")]
        public string PgNb { get; set; }

        [XmlElement("LastPgInd")]
        public string LastPgInd { get; set; }

    }

    [XmlRoot(ElementName = "Acct")]
    public class Account
    {
        [XmlElement("Id")]

        public ID1 Id { get; set; }

        [XmlElement("Ownr")]
        public Owner Ownr { get; set; }
    }

    [XmlRoot(ElementName = "Id")]
    public class ID1
    {
        [XmlElement("Othr")]
        public Other Othr { get; set; }
    }

    [XmlRoot(ElementName = "Othr")]

    public class Other
    {
        [XmlElement("Id")]

        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "Ownr")]

    public class Owner
    {
        [XmlElement("Id")]

        public ID2 Id { get; set; }

    }
    [XmlRoot(ElementName = "Id")]
    public class ID2
    {
        [XmlElement("OrgId")]
        public OrganisationId OrgId { get; set; }
    }

    [XmlRoot(ElementName = "OrgId")]
    public class OrganisationId
    {
        public string AnyBIC { get; set; }

    }

    [XmlRoot(ElementName = "Bal")]
    public class Balance
    {
        [XmlElement("Tp")]
        public Type Tp { get; set; }
        public string Amt { get; set; }
        public string CdtDbtInd { get; set; }

        [XmlElement("Dt")]
        public Date Dt { get; set; }

    }

    [XmlRoot(ElementName = "Tp")]
    public class Type
    {
        [XmlElement("CdOrPrtry")]
        public CreditOrderPerEntry CdOrPrtry { get; set; }
    }

    public class Date
    {
        public string Dt { get; set; }
    }

    [XmlRoot(ElementName = "Cd")]
    public class CreditOrderPerEntry
    {
        [XmlElement("Cd")]
        public string Cd { get; set; }
    }

    [XmlRoot(ElementName = "TxsSummry")]
    public class TextSummary
    {
        [XmlElement("TtlNtries")]
        public totalEntries TtlNtries { get; set; }

        [XmlElement("TtlCdtNtries")]
        public totalCrEntries TtlCdtNtries { get; set; }

        public totalDebitEntries TtlDbtNtries { get; set; }
    }


    public class totalEntries
    {
        public string NbOfNtries { get; set; }

        public string Sum { get; set; }

        [XmlElement("TtlNetNtry")]
        public ToatalNetEntry TtlNetNtry { get; set; }

    }

    public class ToatalNetEntry
    {
        public string Amt { get; set; }

        public string CdtDbtInd { get; set; }
    }
    public class totalCrEntries
    {
        public string NbOfNtries { get; set; }
        public string Sum { get; set; }

    }
    public class totalDebitEntries
    {
        public string NbOfNtries { get; set; }
        public string Sum { get; set; }
    }

    [XmlRoot(ElementName = "Ntry")]
    public class NumberEntry
    {
        public string NtryRef { get; set; }
        public string Amt { get; set; }
        public string CdtDbtInd { get; set; }

        [XmlElement("Sts")]
        public Stats Sts { get; set; }

        [XmlElement("ValDt")]
        public Valdation ValDt { get; set; }

        [XmlElement("BkTxCd")]
        public BookToCredit BkTxCd { get; set; }

        [XmlElement("NtryDtls")]

        public EntryDetails NtryDtls { get; set; }

    }
    [XmlRoot(ElementName = "Sts")]
    public class Stats
    {
        public string Cd { get; set; }
    }
    public class Valdation
    {
        public string Dt { get; set; }
    }

    public class BookToCredit
    {
        [XmlElement("Prtry")]
        public PerEntry Prtry { get; set; }

    }

    public class PerEntry
    {
        public string Cd { get; set; }
    }

    [XmlRoot(ElementName = "NtryDtls")]
    public class EntryDetails
    {
        [XmlElement("TxDtls")]
        public TextDetails TxDtls { get; set; }

    }

    [XmlRoot(ElementName = "TxDtls")]
    public class TextDetails
    {
        [XmlElement("Refs")]
        public Reference Refs { get; set; }

        public string Amt { get; set; }

        public string CdtDbtInd { get; set; }

        [XmlElement("RltdPties")]
        public Retailedparties RltdPties { get; set; }

    }

    [XmlRoot(ElementName = "Refs")]
    public class Reference
    {
        public string MsgId { get; set; }
        public string AcctSvcrRef { get; set; }
        public string InstrId { get; set; }
        public string EndToEndId { get; set; }
        public string TxId { get; set; }

    }

    [XmlRoot(ElementName = "RltdPties")]
    public class Retailedparties
    {
        [XmlElement("InitgPty")]

        public initialgParty InitgPty { get; set; }
    }

    [XmlRoot(ElementName = "InitgPty")]
    public class initialgParty
    {
        [XmlElement("Agt")]

        public agt Agt { get; set; }
    }

    [XmlRoot(ElementName = "InitgPty")]
    public class agt
    {
        [XmlElement("FinInstnId")]
        public FinanceInstituteIndex FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "FinInstnId")]
    public class FinanceInstituteIndex
    {
        public string BICFI { get; set; }
    }

}
