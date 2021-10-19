/* 
    Дополнительный файл перевода, для расширения kendo.messages.ru-RU.js
*/

(function (f) {
    if (typeof define === 'function' && define.amd) {
        define(["kendo.core"], f);
    } else {
        f();
    }
}(function () {
    (function ($, undefined) {
        /* Filter menu operator messages */
        if (kendo.ui.FilterMenu) {
            kendo.ui.FilterMenu.prototype.options.messages =
                $.extend(true, kendo.ui.FilterMenu.prototype.options.messages, {
                    "filter": "фильтровать",
                    "and": "И",
                    "clear": "очистить",
                    "info": "Строки со значениями",
                    "selectValue": "-выберите-",
                    "isFalse": "ложь",
                    "isTrue": "истина",
                    "or": "или",
                    "cancel": "Отмена",
                    "operator": "Оператор",
                    "value": "Значение"
                });
        }


        if (kendo.ui.FilterCell) {
            kendo.ui.FilterCell.prototype.options.messages =
                $.extend(true, kendo.ui.FilterCell.prototype.options.messages, {
                    "isFalse": "нет",
                    "isTrue": "да"
                });
        }

        if (kendo.ui.FilterMenu) {
            kendo.ui.FilterMenu.prototype.options.messages =
                $.extend(true, kendo.ui.FilterMenu.prototype.options.messages, {
                    "isFalse": "нет",
                    "isTrue": "да",
                    "filter": "применить"
                });
        }
        if (kendo.ui.FilterMultiCheck) {
            kendo.ui.FilterMultiCheck.prototype.options.messages =
                $.extend(true, kendo.ui.FilterMultiCheck.prototype.options.messages, {
                    "checkAll": "Выбрать все",
                    "clear": "Очистить",
                    "filter": "Применить",
                    "selectedItemsFormat": "Выбрано позиций: {0}"
                });
        }
        if (kendo.ui.Spreadsheet) {
            kendo.spreadsheet.messages.dialogs = {
                apply: 'Применить',
                save: 'Сохранить',
                cancel: 'Отмена',
                remove: 'Удалить',
                retry: 'Повторить',
                revert: 'Вернуть',
                okText: 'OK',
                formatCellsDialog: {
                    title: 'Формат',
                    categories: {
                        number: 'Номер',
                        currency: 'Валюта',
                        date: 'Дата'
                    }
                },
                fontFamilyDialog: { title: 'Шрифт' },
                fontSizeDialog: { title: 'Размер шрифта' },
                bordersDialog: { title: 'Границы' },
                alignmentDialog: {
                    title: 'Выравнивание',
                    buttons: {
                        justtifyLeft: 'Выровнять по левому краю',
                        justifyCenter: 'По центру',
                        justifyRight: 'Выровнять по правому краю',
                        justifyFull: 'Выровнять по ширине',
                        alignTop: 'Выровнять по верхнему краю',
                        alignMiddle: 'Выровнять по середине',
                        alignBottom: 'Выровнять по нижнему краю'
                    }
                },
                mergeDialog: {
                    title: 'Объединить ячейки',
                    buttons: {
                        mergeCells: 'Объединить все',
                        mergeHorizontally: 'Объединить по строкам',
                        mergeVertically: 'Объединить по колонкам',
                        unmerge: 'Отменить объединение'
                    }
                },
                freezeDialog: {
                    title: 'Заморозить панели',
                    buttons: {
                        freezePanes: 'Заморозить панели',
                        freezeRows: 'Заморозить строки',
                        freezeColumns: 'Заморозить колонки',
                        unfreeze: 'Убрать заморозку'
                    }
                },
                confirmationDialog: {
                    text: 'Вы действительно хотите удалить этот лист?',
                    title: 'Удаление листа'
                },
                validationDialog: {
                    title: 'Проверка данных',
                    hintMessage: 'Введите действительное {0} значение {1}.',
                    hintTitle: 'Валидация {0}',
                    criteria: {
                        any: 'Любое значение',
                        number: 'Число',
                        text: 'Текст',
                        date: 'Дата',
                        custom: 'Пользовательская формула',
                        list: 'Список'
                    },
                    comparers: {
                        greaterThan: 'больше',
                        lessThan: 'меньше',
                        between: 'между',
                        notBetween: 'не между',
                        equalTo: 'равно',
                        notEqualTo: 'не равно',
                        greaterThanOrEqualTo: 'больше или равно',
                        lessThanOrEqualTo: 'меньше или равно'
                    },
                    comparerMessages: {
                        greaterThan: 'больше {0}',
                        lessThan: 'меньше {0}',
                        between: 'между {0} и {1}',
                        notBetween: 'не между {0} и {1}',
                        equalTo: 'равно {0}',
                        notEqualTo: 'не равно {0}',
                        greaterThanOrEqualTo: 'больше или равно {0}',
                        lessThanOrEqualTo: 'меньше или равно {0}',
                        custom: 'удовлетворяет формуле: {0}'
                    },
                    labels: {
                        criteria: 'Критерии',
                        comparer: 'Сравнение',
                        min: 'Миннимум',
                        max: 'Максимум',
                        value: 'Значение',
                        start: 'От',
                        end: 'До',
                        onInvalidData: 'Недействительные данные',
                        rejectInput: 'Отклонить ввод',
                        showWarning: 'Показать предупреждение',
                        showHint: 'Показать подсказку',
                        hintTitle: 'Подсказка',
                        hintMessage: 'Текст подсказки',
                        ignoreBlank: 'Игнорировать не заполненные',
                        showListButton: 'Показать кнопку для отображения списка',
                        showCalendarButton: 'Кнопка отображения для отображения календаря'
                    },
                    placeholders: {
                        typeTitle: 'Тип заголовка',
                        typeMessage: 'Тип сообщения'
                    }
                },
                exportAsDialog: {
                    title: 'Экспорт...',
                    labels: {
                        scale: 'Масштаб',
                        fit: 'Подгонка к странице',
                        fileName: 'Имя файла',
                        saveAsType: 'Сохранить как',
                        exportArea: 'Экспорт',
                        paperSize: 'Размер листа',
                        margins: 'Поля',
                        orientation: 'Ориентация',
                        print: 'Печать',
                        guidelines: 'Методические рекомендации',
                        center: 'Центр',
                        horizontally: 'Горизонтально',
                        vertically: 'Вертикально'
                    }
                },
                modifyMergedDialog: { errorMessage: 'Невозможно изменить часть объединенной ячейки.' },
                rangeDisabledDialog: { errorMessage: 'Целевой диапазон содержит отключенные ячейки.' },
                intersectsArrayDialog: { errorMessage: ' не можете изменить часть массива' },
                incompatibleRangesDialog: { errorMessage: 'Несовместимые диапазоны' },
                noFillDirectionDialog: { errorMessage: 'Невозможно определить направление заполнения' },
                duplicateSheetNameDialog: { errorMessage: 'Дублируемое имя листа' },
                overflowDialog: {
                    errorMessage: 'Нельзя вставить, потому что область копирования и область вставки не имеют одинакового размера и формы.'
                },
                useKeyboardDialog: {
                    title: 'Копирование и вставка',
                    errorMessage:
                        'Эти действия не могут быть вызваны через меню. Вместо этого используйте сочетания клавиш:',
                    labels: {
                        forCopy: 'для копирования',
                        forCut: 'для вырезания',
                        forPaste: 'для вставки'
                    }
                },
                unsupportedSelectionDialog: { errorMessage: 'Это действие не может выполняться при множественном выборе.' },
                linkDialog: {
                    title: 'Гиперссылка',
                    labels: {
                        text: 'Текст',
                        url: 'Адрес',
                        removeLink: 'Удалить ссылку'
                    }
                }
            }
        }


    })(window.kendo.jQuery);
}));

