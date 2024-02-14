# BankAccount
Entity Framework Core не работал в UWP (старые версию включительно), писал без ORM и из за этого получилось много сервисов. 

Отдельно не проектировал базу данных, выходило отношение многие ко многим, остановился на варианте, где бизнес сущности не имеют связей между друг другом (так как задач для этого и не было, могу быстро исправить). 

В стандартных элементах UWP Controls не было DataGrid. Установил Microsoft.Toolkit.Uwp.UI.Controls.DataGrid, показывает 4 предупреждения, но работает. 


MVVM:


![Screenshot_2](https://github.com/Gladn/BankAccount/assets/92585647/26300417-1835-4efb-87a3-d9d617db8d73)

Скриншоты:

![изображение](https://github.com/Gladn/BankAccount/assets/92585647/cf558566-c8c5-401a-96ea-97337fda8065)


![изображение](https://github.com/Gladn/BankAccount/assets/92585647/4670ce0c-ea7a-43a7-b70d-2f2047365691)


![изображение](https://github.com/Gladn/BankAccount/assets/92585647/7a7d0e15-8f3d-4cb6-b833-97a3711e69f6)


![изображение](https://github.com/Gladn/BankAccount/assets/92585647/0ef64434-b63a-4741-9204-2a04bed941fa)


![изображение](https://github.com/Gladn/BankAccount/assets/92585647/b2ef840c-2436-4953-ace1-476b17c6dcae)
