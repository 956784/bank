using System;
using System.Threading;

class Program
{
    static void Main()
    {
        var account = new Account(); // Создаем экземпляр класса Account
        const decimal requiredAmount = 100m; // Задаем необходимую сумму для снятия (100)

        // Создаем новый поток для периодического внесения случайных сумм на счет
        var depositThread = new Thread(() =>
        {
            var random = new Random(); // Инициализируем генератор случайных чисел
            for (int i = 0; i < 3; i++) // Внесение будет происходить 3 раза
            {
                decimal amountToDeposit = random.Next(10, 60); // Генерируем случайную сумму от 10 до 60
                account.Deposit(amountToDeposit); // Вносим сгенерированную сумму на счет
                Thread.Sleep(1000); // Задержка между внесениями (1 секунда)
            }
        });

        depositThread.Start(); // Запускаем поток для внесений

        // Ожидаем, пока баланс счета не достигнет необходимой суммы для снятия
        account.WaitForBalance(requiredAmount);

        // Пытаемся снять необходимую сумму
        if (account.TryWithdraw(requiredAmount))
        {
            Console.WriteLine($"Остаток на счете: {account.Balance:C}"); // Показываем остаток на счету после снятия
        }
    }
}

class Account
{
    private decimal _balance; // Поле для хранения баланса
    private readonly object _balanceLock = new object(); // Объект для синхронизации доступа к балансу

    public decimal Balance
    {
        get
        {
            lock (_balanceLock) // Блокировка для безопасного доступа к балансу
            {
                return _balance; // Возвращаем текущий баланс
            }
        }
    }

    public void Deposit(decimal amount)
    {
        lock (_balanceLock) // Блокировка для безопасного внесения средств
        {
            _balance += amount; // Увеличиваем баланс на внесенную сумму
            Console.WriteLine($"Внесение: {amount:C}, Текущий баланс: {Balance:C}"); // Выводим информацию о внесении
        }
    }

    public bool TryWithdraw(decimal amount)
    {
        lock (_balanceLock) // Блокировка для безопасного снятия средств
        {
            if (_balance >= amount) // Проверяем, достаточно ли средств для снятия
            {
                _balance -= amount; // Уменьшаем баланс на сумму снятия
                Console.WriteLine($"Снятие: {amount:C}, Новый баланс: {Balance:C}"); // Выводим информацию о снятии
                return true; // Возвращаем true, если снятие прошло успешно
            }
            else
            {
                Console.WriteLine($"Недостаточно средств для снятия: {amount:C}. Текущий баланс: {Balance:C}"); // Выводим предупреждение
                return false; // Возвращаем false, если недостаточно средств
            }
        }
    }

    public void WaitForBalance(decimal amount)
    {
        while (true) // Бесконечный цикл ожидания
        {
            lock (_balanceLock) // Блокировка для безопасной проверки баланса
            {
                if (_balance >= amount) // Если баланс достаточный
                {
                    break; // Выход из цикла
                }
            }
            Thread.Sleep(500); // Задержка проверки (0.5 секунды)
        }
    }
}