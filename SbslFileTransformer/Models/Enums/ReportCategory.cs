using System.ComponentModel;

namespace SbslFileTransformer.Models.Enums
{
    public enum ReportCategory
    {
        Default,
        [Description("Nostro")] Nostro,
        [Description("CBK")] CBK,
        [Description("BNR")] BNR,
        [Description("Western,Union")] WesternUnion,
        [Description("Moneygram")] Moneygram,
        [Description("Branch,Suspense")] BranchSuspense,
        [Description("Airtel,B2W")] AirtelB2W,
        [Description("Momo,Float")] MomoFloat,
        [Description("MTN,Airtime")] MTNAirtime,
        [Description("MTN,PushPull")] MTNPushPull,
        [Description("Spenn,CashInOut")] SPENNCashInOut,
        [Description("FDI")] FDI,
        [Description("DSTV")] DSTV,
        [Description("CDM")] CDM,
        [Description("Mobile,Banking")] MobileBanking,
        [Description("Clearing,Suspense")] ClearingSuspense,
        [Description("Credit")] Credit,
        [Description("Central,Operations")] CentralOperations,
        [Description("Finance")] Finance,
        [Description("AirtelB2C_C2B")] AirtelB2CC2B,
        [Description("Mpesa,C2B")] MpesaC2B,
        [Description("Mpesa,B2C,Omni")] MpesaB2COmni,
        [Description("Mpesa,B2C,Chango")] MpesaB2CChango,
        [Description("Mpesa,B2C,Elma")] MpesaB2CElma,
        [Description("Mpesa,Bank,To,Till")] MpesaBanktoTill,
        [Description("Mpesa,C2B,Merchant")] MpesaC2BMerchant,
        [Description("Mpesa,Lipa,na")] MpesaLipaNaMpesa,
        [Description("Mpesa,C2B,Chango")] MpesaC2BChango,
        [Description("IMS")] IMS,
        [Description("Fx,Confirmation,Spot")] FXConfirmationSpot,
        [Description("Fx,Confirmation,Money,Market")] FXConfirmationMoneyMarket,
        [Description("Bill,Payments")] BillPayments,
        [Description("Fin,Prepayments")] FinancePrepayments,
        [Description("Fin,Payables")] FinancePayables,
        [Description("Mpesa,B2C")] MpesaB2C,
        [Description("Airtel,B2C")] AirtelB2C,
        [Description("Airtel,C2B")] AirtelC2B,
        [Description("MT,Western,Union")] MoneyTransfersWesternUnion,
        [Description("MT,MoneyGram")] MoneyTransfersMoneyGram
    }
}