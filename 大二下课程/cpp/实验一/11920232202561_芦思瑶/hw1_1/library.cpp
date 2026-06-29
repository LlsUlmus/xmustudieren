#include "library.h"

// 用于处理 const 对象获取 ID
int get_ID_const(const Book& book) 
{
    Book& non_const_book = const_cast<Book&>(book);
    return non_const_book.get_ID();
}

// 用于处理 const 对象打印信息
void print_message_const(const Book& book) 
{
    Book& non_const_book = const_cast<Book&>(book);
    non_const_book.print_message();
}

void Library::add_book(Book book) 
{
    Books.push_back(book);
    cout << "Added:" << book.get_name() << endl;
}

void Library::delete_book(int book_ID) 
{
    for (auto it = Books.begin(); it != Books.end(); ++it) 
    {
        if (it->get_ID() == book_ID) 
        {
            cout << "Deleted:" << it->get_name() << endl;
            Books.erase(it);
            return;
        }
    }
    cout << "Delete failed" << endl;
}


void Library::find_book(int book_ID) 
{
    for (const auto& book : Books) 
    {
        if (get_ID_const(book) == book_ID) 
        {
            print_message_const(book);
            return;
        }
    }
    cout << "Find Failed" << endl;
}

void Library::borrow_book(int book_ID, string borrower) 
{
    for (auto& book : Books)
    {
        if (book.get_ID() == book_ID) 
        {
            if (!book.get_state()) 
            {
                book.change_state(borrower);
                cout << "Borrowed:" << book.get_name() << endl;
            }
            else 
            {
                cout << "Borrow Failed" << endl;
            }
            return;
        }
    }
    cout << "Borrow Failed" << endl;
}

void Library::return_book(int book_ID) 
{
    for (auto& book : Books) 
    {
        if (book.get_ID() == book_ID)
        {
            if (book.get_state()) 
            {
                book.change_state();
                cout << "Returned:" << book.get_name() << endl;
            }
            else 
            {
                cout << "Return Failed" << endl;
            }
            return;
        }
    }
    cout << "Return Failed" << endl;
}