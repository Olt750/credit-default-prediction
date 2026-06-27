export type FinancialProfileForm = {
  age: string;
  annualIncome: string;
  loanAmount: string;
  creditScore: string;
  employmentStatus: string;
  loanTermMonths: string;
  previousDefaults: string;
  education: string;
  maritalStatus: string;
  monthlyCarLoanPayment: string;
  monthlyMortgageOrRentPayment: string;
  monthlyPersonalLoanPayment: string;
  monthlyCreditCardPayment: string;
  monthlyOtherDebtPayment: string;
};

export const initialFinancialProfileForm: FinancialProfileForm = {
  age: "",
  annualIncome: "",
  loanAmount: "",
  creditScore: "",
  employmentStatus: "Employed",
  loanTermMonths: "",
  previousDefaults: "0",
  education: "High school",
  maritalStatus: "Single",
  monthlyCarLoanPayment: "0",
  monthlyMortgageOrRentPayment: "0",
  monthlyPersonalLoanPayment: "0",
  monthlyCreditCardPayment: "0",
  monthlyOtherDebtPayment: "0",
};

export type FinancialProfileErrors = Partial<Record<keyof FinancialProfileForm, string>>;

const numberOrZero = (value: string) => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : 0;
};

export function calculateFinancials(form: FinancialProfileForm) {
  const annualIncome = numberOrZero(form.annualIncome);
  const monthlyIncome = annualIncome > 0 ? annualIncome / 12 : 0;
  const totalMonthlyDebt =
    numberOrZero(form.monthlyCarLoanPayment) +
    numberOrZero(form.monthlyMortgageOrRentPayment) +
    numberOrZero(form.monthlyPersonalLoanPayment) +
    numberOrZero(form.monthlyCreditCardPayment) +
    numberOrZero(form.monthlyOtherDebtPayment);
  const debtToIncomeRatio = monthlyIncome > 0 ? totalMonthlyDebt / monthlyIncome : 0;

  return {
    monthlyIncome,
    totalMonthlyDebt,
    debtToIncomeRatio,
  };
}

export function validateFinancialProfile(form: FinancialProfileForm) {
  const errors: FinancialProfileErrors = {};
  const age = Number(form.age);
  const annualIncome = Number(form.annualIncome);
  const loanAmount = Number(form.loanAmount);
  const creditScore = Number(form.creditScore);
  const loanTermMonths = Number(form.loanTermMonths);
  const previousDefaults = Number(form.previousDefaults);
  const debtFields: (keyof FinancialProfileForm)[] = [
    "monthlyCarLoanPayment",
    "monthlyMortgageOrRentPayment",
    "monthlyPersonalLoanPayment",
    "monthlyCreditCardPayment",
    "monthlyOtherDebtPayment",
  ];

  if (!Number.isFinite(age) || age < 18) errors.age = "Age must be at least 18.";
  if (!Number.isFinite(annualIncome) || annualIncome <= 0) errors.annualIncome = "Annual income must be greater than 0.";
  if (!Number.isFinite(loanAmount) || loanAmount <= 0) errors.loanAmount = "Loan amount must be greater than 0.";
  if (!Number.isFinite(creditScore) || creditScore < 300 || creditScore > 850) errors.creditScore = "Credit score must be between 300 and 850.";
  if (!Number.isFinite(loanTermMonths) || loanTermMonths <= 0) errors.loanTermMonths = "Loan term must be greater than 0.";
  if (!Number.isFinite(previousDefaults) || previousDefaults < 0) errors.previousDefaults = "Previous defaults cannot be negative.";

  debtFields.forEach((field) => {
    const value = Number(form[field]);
    if (!Number.isFinite(value) || value < 0) errors[field] = "Monthly debt cannot be negative.";
  });

  return errors;
}

export function toFinancialProfilePayload(form: FinancialProfileForm) {
  const financials = calculateFinancials(form);

  return {
    age: Number(form.age),
    annualIncome: Number(form.annualIncome),
    loanAmount: Number(form.loanAmount),
    creditScore: Number(form.creditScore),
    employmentStatus: form.employmentStatus,
    loanTermMonths: Number(form.loanTermMonths),
    loanTerm: Number(form.loanTermMonths),
    previousDefaults: Number(form.previousDefaults),
    education: form.education,
    maritalStatus: form.maritalStatus,
    monthlyCarLoanPayment: Number(form.monthlyCarLoanPayment),
    monthlyMortgageOrRentPayment: Number(form.monthlyMortgageOrRentPayment),
    monthlyPersonalLoanPayment: Number(form.monthlyPersonalLoanPayment),
    monthlyCreditCardPayment: Number(form.monthlyCreditCardPayment),
    monthlyOtherDebtPayment: Number(form.monthlyOtherDebtPayment),
    totalMonthlyDebt: financials.totalMonthlyDebt,
    debtToIncomeRatio: Number(financials.debtToIncomeRatio.toFixed(4)),
  };
}
