using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace OOPExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var banks = GenerateBanks();
            var users = GenerateUsers(banks);

            //1) Сделать выборку всех Пользователей, имя + фамилия которых длиннее чем 12 символов.
            var lessThen12SymbolsLength = users
              .Where(x => (x.FirstName + x.LastName).Length > 12)
              .ToList();
            //2) Сделать выборку всех транзакций (в результате должен получится список из 1000 транзакций)           
            
            var allTranasctionsList = users
                .SelectMany(user => user.Transactions)
                .ToList();

            //3) Вывести Банк: и всех его пользователей (Имя + фамилия + количество транзакий в гривне) отсортированных по Фамилии по убиванию. в таком виде :
            //   Имя банка 
            //   ***************
            //   Игорь Сердюк 
            //   Николай Басков
            var usersBanks = users                
                .GroupBy(x => x.Bank);

            foreach (var group in usersBanks)
            {
                Console.WriteLine(group.Key.Name);
                Console.WriteLine("****************");

                foreach (var user in group.OrderBy(x => x.LastName))
                {
                    Console.WriteLine($"{user.FirstName} + {user.LastName} + {user.Transactions.Count(y => y.Currency == Currency.UAH)}");
                }
            }

            //4) Сделать выборку всех Пользователей типа Admin, у которых счет в банке, в котором больше всего транзакций


            var userAdmins = users
                .Where(user => user.Type == UserType.Admin);

            var bankWithMostTranasctions = banks
                .OrderBy(bank => bank.Transactions.Count)
                .ThenBy(bank => bank.Name)
                .FirstOrDefault();


            var adminsInBankWihMostTransactions = userAdmins
                .Where(x => x.Bank == bankWithMostTranasctions)
                .ToList();


            //5) Найти Пользователей(НЕ АДМИНОВ), которые произвели больше всего транзакций в определенной из валют (UAH,USD,EUR) 
            //то есть найти трёх пользователей: 1й который произвел больше всего транзакций в гривне, второй пользователь, который произвел больше всего транзакций в USD 
            //и третьего в EUR

            var notAdmins = users
                 .Where(user => user.Type != UserType.Admin);

            var userWithmostTransactionInUAH = notAdmins
                .OrderByDescending(x => x.Transactions.Where(y => y.Currency == Currency.UAH).ToList().Count)
                .FirstOrDefault();

            var userWithmostTransactionInUSD = notAdmins
                .OrderByDescending(x => x.Transactions.Count(y => y.Currency == Currency.USD))
                .FirstOrDefault();

            var userWithmostTransactionInEUR = notAdmins
                .OrderByDescending(x => x.Transactions.Count(y => y.Currency == Currency.EUR))
                .FirstOrDefault();
         
            //6) Сделать выборку транзакций банка, у которого больше всего Pemium пользователей

            var premiumUsers = users
                .Where(x => x.Type == UserType.Pemium);

            var bankWithMostPremiumUsers = premiumUsers
                .GroupBy(x => x.Bank)
                .OrderByDescending(x => x.Key.Transactions.Count)
                .FirstOrDefault()
                .Key
                .Name;
               
            var transactionsOfBankWithMostPremiumUsers = banks
                .Where(x => x.Name == bankWithMostPremiumUsers)
                .SelectMany(x => x.Transactions)    
                .ToList();
        
            Console.ReadKey();
        }

        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Transaction> Transactions { get; set; }
            public UserType Type { get; set; }
            public Bank Bank { get; set; }
        }

        public class Transaction
        {
            public decimal Value { get; set; }

            public Currency Currency { get; set; }
        }

        public static List<Transaction> GetTenTransactions()
        {
            var result = new List<Transaction>();
            var sign = random.Next(0, 1);
            var signValue = sign == 0 ? -1 : 1;
            for (var i = 0; i < 10; i++)
            {
                result.Add(new Transaction
                {
                    Value = (decimal)random.NextDouble() * signValue * 100m,
                    Currency = GetRandomCurrency(),
                });
            }

            return result;
        }

        public class Bank
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Transaction> Transactions { get; set; }

           
        }

        public enum UserType
        {
            Default = 1,
            Pemium = 2,
            Admin = 3
        }

        public static UserType GetRandomUserType()
        {
            int userTypeInt = random.Next(1, 4);

            return (UserType)userTypeInt;
        }

        public enum Currency
        {
            USD = 1,
            UAH = 2,
            EUR = 3
        }

        public static Currency GetRandomCurrency()
        {
            int userTypeInt = random.Next(1, 4);

            return (Currency)userTypeInt;
        }

        public static List<Bank> GenerateBanks()
        {
            var banksCount = random.Next(BANKS_MIN, BANKS_MAX);
            var result = new List<Bank>();

            for (int i = 0; i < banksCount; i++)
            {
                result.Add(new Bank
                {
                    Id = i + 1,
                    Name = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Transactions = new List<Transaction>()
                });
            }

            return result;
        }

        public static List<User> GenerateUsers(List<Bank> banks)
        {
            var result = new List<User>();
            int bankId = 0;
            Bank bank = null;
            List<Transaction> transactions = null;
            for (int i = 0; i < 100; i++)
            {
                bankId = random.Next(0, banks.Count);
                bank = banks[bankId];
                transactions = GetTenTransactions();
                result.Add(new User
                {
                    Bank = bank,
                    FirstName = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Id = i + 1,
                    LastName = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Type = GetRandomUserType(),
                    Transactions = transactions
                });
                bank.Transactions.AddRange(transactions);
            }

            return result;
        }

        private const int BANKS_MIN = 2;
        private const int BANKS_MAX = 5;

        private const int NAME_MAX_LENGTH = 10;
        private const int NAME_MIN_LENGTH = 4;

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}