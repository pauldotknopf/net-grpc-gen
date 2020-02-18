#ifndef GENPROTO_TEST1_H
#define GENPROTO_TEST1_H
#include <QObject>
namespace Tests {

struct QTest1Private;

class QTest1 : public QObject {
	Q_OBJECT
public:
	QTest1(QObject* parent = nullptr);
	~QTest1();
private:
    QScopedPointer<QTest1Private> const d_priv;
};
}
#endif // GENPROTO_TEST1_H
