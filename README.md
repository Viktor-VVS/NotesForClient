# NotesForClient
Позволяет создавать ,редактировать,удалять заметки по клиентам .Мониторит их состояние.
Смысл программы следующий: через нее мы записываем и просматриваем заметки о клиентах.
Рядом с исполняемым файлом находится файл settings.ini. В нем указана папка где будут хранится все заметки по клиентам.в формате .txt
Во время запуска программа мониторит папку на наличие заметок и выдает уже найденные в список.
В поле Client  записываем имя клиента и ниже сам текст заметок.
При нажатии Save сохранится новая заметка.Обновится список.
Можно набрать в поле Client имя существующей заметки и нажать Load подтянется содержимое этой заметки.
Двойным щелчком на заметке в списке также загрузит ее содержимое.
Существует поиск по имени заметки Search by Client а также поиск по содержимому заметки Search by content
При сворачивании окно сворачивается с Трей. с контекстным меню Показать и Закрыть.
Программу можно использовать по локальной сети. указав в settings.ini  путь по сети к папке.
т.е возможно указать общую папку для клиентов и с разных компьютеров подключенных к локальной сети одновременно смотреть и править заметки.
Изменения сразу будут отображены у всех пользователей сразу.
Также при открытии заметки сверху будет показана надпись кто последний раз редактировал заметку и дата и время редактирования.