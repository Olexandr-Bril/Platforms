# SplitBill

Навчальний проєкт — спрощена копія Splitwise. Web API на .NET 9 + Entity Framework Core (SQLite).
Можна створити групу, додавати спільні витрати і дивитись, хто кому скільки винен.

## Функціонал

- реєстрація і вхід (JWT токен)
- створення групи, додавання учасників по email
- додавання витрат: порівну (Equal) або точними сумами (Exact)
- баланси і список "хто кому скільки має віддати"
- повернення боргу (payment)

## Архітектура

Проєкт зроблений по трирівневій архітектурі:

- `SplitBill.Data` — рівень доступу до даних: сутності (User, Group, Expense, ...) і `AppDbContext`
- `SplitBill.Business` — бізнес-логіка: сервіси, DTO, валідація, підрахунок боргів
- `SplitBill.Api` — рівень представлення: контролери, JWT, обробка помилок
- `SplitBill.Tests` — unit тести (xUnit)

Api -> Business -> Data. Контролери нічого не знають про базу, вони тільки викликають сервіси.

## Запуск

```
dotnet build
dotnet test
dotnet run --project SplitBill.Api
```

API запускається на http://localhost:5100, файл бази `splitbill.db` створюється автоматично.
Приклади запитів є у файлі `SplitBill.Api/SplitBill.Api.http`.

## Ендпоїнти

| Метод | Шлях | Опис |
|---|---|---|
| POST | /api/auth/register | реєстрація |
| POST | /api/auth/login | вхід, повертає токен |
| POST | /api/groups | створити групу |
| GET | /api/groups | мої групи |
| GET | /api/groups/{id} | інформація про групу |
| POST | /api/groups/{id}/members | додати учасника по email |
| POST | /api/groups/{id}/expenses | додати витрату |
| GET | /api/groups/{id}/expenses | всі витрати групи |
| GET | /api/groups/{id}/balances | баланси і борги |
| POST | /api/groups/{id}/payments | повернути борг |

Помилки: 400 — неправильні дані, 401 — не авторизований, 403 — не учасник групи, 404 — не знайдено.

## Тести

xUnit + EF Core InMemory. Перевіряється підрахунок боргів (`DebtCalculator`), сервіс груп, витрати і баланси.

## CI

`.github/workflows/splitbill.yml` (в корені репозиторію) — GitHub Actions: при push в main проєкт збирається і запускаються тести.
