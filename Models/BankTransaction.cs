using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salton.Models
{
    public class StoreData {
        public Store Store { get; set; }

        public List<CashPayment> PreviousMonthData { get; set; } = new List<CashPayment>();
        public List<CashPayment> CurrentMonthData { get; set; } = new List<CashPayment>();
        public List<BankPaymentReconciliation> BankReconciliationResult { get; set; }
    }
    public class Store {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Payment> Payments { get; set; }

    }
    public class Payment
    {
        public PaymentType Type { get; set; }
        public string Expression { get; set; }
        public string ColumnLetter { get; set; }

    }
    public class CashPayment
    {
        public DateTime? Date { get; set; }
        public decimal? NET { get; set; }
        public decimal? GST { get; set; }
        public decimal? PST { get; set; }
        public decimal? Gross { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Debit { get; set; }
        public decimal? GiftCertificateIssued { get; set; }
        public decimal? GiftCertificateRedeemed { get; set; }
        public decimal? Usd { get; set; }
        public decimal? FxRate { get; set; }
        public decimal? CreditNoteIssued { get; set; }
        public decimal? CreditNoteRedeemed { get; set; }
        public decimal? Visa { get; set; }
        public decimal? MasterCard { get; set; }
        public decimal? Amex { get; set; }
        public decimal? Discover { get; set; }
        public decimal? Union { get; set; }
        public decimal? Gc { get; set; }
        public decimal? UnKnow { get; set; }
        public decimal? Total { get; set; }
        public decimal? Diff { get; set; }
    }

    public class BankTransaction
    {
        public DateTime? Date { get; set; }
        public string Description { get; set; }

        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Currency { get; set; }
        public bool SumOff { get; set; }
        public int RowNumber { get; set; }
    }
    public class CashTransaction
    {
        public DateTime? Date { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public bool Matched { get; set; }
    }
    public class BankPaymentReconciliation
    {
        public PaymentType Type { get; set; }
        public BankReconciliationResult BankReconciliationResult { get; set; }
    }

    public class BankReconciliationRecord
    {
        public BankTransaction BankTransaction { get; set; }
        public CashTransaction CashTransaction { get; set; }
    }
    public class BankReconciliationResult 
    {
        public IEnumerable<BankReconciliationRecord> BankReconciliationRecords { get; set; }

        public decimal CurrentMonthAmount { get; set; }
        public decimal PreviousMonthOutStanding { get; set; }
        public decimal CurrentMonthOutStanding { get; set; }
        public IEnumerable<CashTransaction> CurrentMonthOutStandingRecords { get;  set; }
        public IEnumerable<CashTransaction> PreviousMonthOutStandingRecords { get;  set; }
        public IEnumerable<CashTransaction> CurrentMonthAmountRecords { get;  set; }
    }
    public class BankFileMapping 
    {
        public string ThisMonthOutStanding { get; set; }
    }
}
